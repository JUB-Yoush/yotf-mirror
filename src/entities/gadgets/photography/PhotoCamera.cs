using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Yotf;

public partial class PhotoCamera : Item
{
    private static readonly Texture2D moonin = GD.Load<Texture2D>("res://assets/2d/mooninicon.png");

    public static new readonly PackedScene Packed = GD.Load<PackedScene>("uid://cgk7l4ybjl37y");

    public List<Photo> Photos = [];

    private bool equipped = false;
    const float DefaultFov = 90;
    const float ViewfinderFov = 50;
    const float ViewfinderLerp = 20;

    private Camera3D camera = null!;
    private TextureRect photoLetterBox = null!;
    private ColorRect flashRect = null!;
    private PhotoTerminal? photoTerminal = null;
    private Camera3D photoCamera = null!;
    private SubViewport photoViewport = null!;
    private MeshInstance3D mesh = null!;
    private Inventory inventory = null!;

    private PlayerController player = null!;

    private int film = 0;
    private int maxFilm = 0;

    private bool aiming = false;

    public override void _Ready()
    {
        ItemName = "camera";
        player = GetParent().GetParent<PlayerController>();
        camera = player.GetNode<Camera3D>("%Camera3D");
        photoLetterBox = GetNode<TextureRect>("%PhotoLetterBox");
        flashRect ??= GetNode<ColorRect>("%FlashRect");
        photoTerminal ??= GetTree().CurrentScene.GetNodeOrNull<PhotoTerminal>("%PhotoTerminal");
        photoViewport = GetNode<SubViewport>("SubViewport");
        photoCamera = photoViewport.GetNode<Camera3D>("PhotoCamera");
        photoViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Disabled;
        mesh = GetNode<MeshInstance3D>("MeshInstance3D");
        inventory = GetParent<Inventory>();
    }

    public override void _Input(InputEvent @event)
    {
        if (!CurrentItem)
            return;

        if (@event.IsActionPressed("look_cam"))
        {
            photoViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
            player.IsInMenu = true;
            aiming = true;
        }

        if (@event.IsActionReleased("look_cam"))
        {
            photoViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Disabled;
            player.IsInMenu = false;
            aiming = false;
        }

        if (@event.IsActionPressed("drop_item"))
        {
            var dropItem = MakeDropItem(mesh.Mesh, Packed);
            dropItem.GlobalTransform = camera.GlobalTransform;
            GetTree().CurrentScene.AddChild(dropItem);
            inventory.RemoveCurrentItem();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!CurrentItem)
            return;

        if (aiming)
        {
            camera.Fov = MathExt.Lerp(camera.Fov, ViewfinderFov, (float)(ViewfinderLerp * delta));
            photoLetterBox.Visible = true;
        }
        else
        {
            camera.Fov = MathExt.Lerp(camera.Fov, DefaultFov, (float)(ViewfinderLerp * delta));
            photoLetterBox.Visible = false;
        }

        if (Input.IsActionJustPressed("take_photo") && aiming)
        {
            var subjects = GetPhotoSubjects();
            Image image = GetViewportImage();
            PhotoData photo = PhotoData.New(Name, subjects, image.Data);
            Dictionary<string, PhotoGrade> grades = GetSubjectGrades(photo);
            FlashSFX();
            AddPhoto(photo, grades);
        }
        mesh.GlobalTransform = camera.GlobalTransform;
        mesh.GlobalPosition += (-mesh.GlobalBasis.Z / 2) + (mesh.GlobalBasis.X / 2); //+ new Vector3(0, 0, 2);
        photoCamera.GlobalTransform = camera.GlobalTransform;
    }

    private Dictionary<string, PhotoGrade> GetSubjectGrades(PhotoData photo)
    {
        Dictionary<string, PhotoGrade> result = [];

        foreach (var subjectName in photo.Subjects)
        {
            // TODO (j) where will fish be placed within the scene?
            var subject = GetTree().CurrentScene.GetNode<Node3D>(subjectName);

            // how centered the fish is
            var camToFish = subject.GlobalPosition - photoCamera.GlobalPosition;
            var camFacing = photoCamera.GlobalTransform.Basis.Z;
            var angle = camFacing.AngleTo(camToFish); // from a range of abt 2.7 - PI
            var angleScore = (angle - 2.6) / (Math.PI - 2.6);

            //If the Fish is facing the camera
            var facingAngle = camFacing.AngleTo(-subject.GlobalTransform.Basis.Z); // from a range of 0 - PI
            var facingScore = (facingAngle / Math.PI);

            // size of fish on the screen
            // distance from camera scaled based on the size of the bounding box
            var vis = subject.GetNode<MeshInstance3D>("MeshInstance3D") as VisualInstance3D; // TODO(j) maybe have a "photoboundingbox" mesh for fish?
            var worldAabb = vis!.GetAabb() * vis.GlobalTransform;
            var sizeInPhoto = worldAabb.Volume / camToFish.Length(); // from a range of 0 - 0.1?
            var sizeScore = Math.Min(sizeInPhoto * 10, 1.0f);

            //fish lighting
            // TODO (j) implement
            var lightScore = 1f;
            result.Add(
                subject.Name,
                new((float)angleScore, sizeScore, (float)facingScore, lightScore)
            );
        }
        return result;
    }

    private Image GetViewportImage()
    {
        var img = photoViewport.GetTexture().GetImage();
        return img;
    }

    private string[] GetPhotoSubjects()
    {
        List<string> result = [];
        foreach (var child in GetTree().CurrentScene.GetChildren(true))
        {
            if (child is IPhotographable photographable && photographable.IsInPhoto())
            {
                result.Add(child.Name);
            }
        }
        return [.. result];
    }

    void FlashSFX()
    {
        var tween = CreateTween();
        tween.Fn(() => flashRect.Visible = true);
        tween.TweenInterval(.1);
        tween.Fn(() => flashRect.Visible = false);
    }

    void TakeScreenShot(string id)
    {
        GetViewport().GetTexture().GetImage().SavePng($"user://live-camera-roll/{id}.png");
    }

    [Rpc(
        MultiplayerApi.RpcMode.AnyPeer,
        CallLocal = true,
        TransferMode = MultiplayerPeer.TransferModeEnum.Reliable,
        TransferChannel = 0
    )]
    public void AddPhoto(string photoJson, string photoTaker)
    {
        Rpc(MethodName.UpdateTerminalImage, photoJson);
        if (photoTaker == Name && player.IsMultiplayerAuthority())
            Log.Print("I took this photo");
        else
            Log.Print("I didn't take this photo");
    }

    public void AddPhoto(PhotoData photoData, Dictionary<string, PhotoGrade> grades)
    {
        var photo = new Photo(photoData, grades);
        Photos.Add(photo);
        UpdateTerminalImage(photo);
    }

    public void UpdateTerminalImage(Photo photo)
    {
        var imgData = photo.Data;
        var photoImg = Image.CreateFromData(
            imgData.Width,
            imgData.Height,
            imgData.Mipmaps,
            Image.Format.Rgb8,
            imgData.Bytes
        );
        var imgTex = new ImageTexture();
        imgTex.SetImage(photoImg);
        photoTerminal?.GetNode<Sprite3D>("Sprite3D").Texture = imgTex;
    }

    [Rpc(
        MultiplayerApi.RpcMode.AnyPeer,
        CallLocal = true,
        TransferMode = MultiplayerPeer.TransferModeEnum.Reliable,
        TransferChannel = 0
    )]
    public void UpdateTerminalImage(string photoJson)
    {
        PhotoData imgData = PhotoData.FromJson(photoJson);
        var photoImg = Image.CreateFromData(
            imgData.Width,
            imgData.Height,
            imgData.Mipmaps,
            Image.Format.Rgb8,
            imgData.Bytes
        );
        var imgTex = new ImageTexture();
        imgTex.SetImage(photoImg);
        photoTerminal?.GetNode<Sprite3D>("Sprite3D").Texture = imgTex;
    }

    public void ClearPhotos()
    {
        Photos = [];
    }
}
