using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class PhotoCamera : Item
{
    public override void _Notification(int what) => this.Notify(what);

    private static readonly Texture2D moonin = GD.Load<Texture2D>("res://assets/2d/mooninicon.png");

    public static new readonly PackedScene Packed = GD.Load<PackedScene>("uid://cgk7l4ybjl37y");

    public List<Photo> Photos = [];

    private bool equipped = false;
    const float DefaultFov = 90;
    const float ViewfinderFov = 50;
    const float ViewfinderLerp = 20;

    [Node]
    public required Camera3D PhotoCameraCam { set; get; }

    [Node]
    public required TextureRect PhotoLetterBox { set; get; }

    [Node]
    public required ColorRect FlashRect { set; get; }

    [Node]
    public required SubViewport PhotoViewport { set; get; }

    [Node]
    public required MeshInstance3D Mesh { set; get; }

    [Node]
    public required SpotLight3D Light { set; get; }

    [Node]
    public required Label FilmLabel { set; get; }

    private PlayerController player = null!;

    private Camera3D playerCamera = null!;

    private PhotoTerminal? photoTerminal = null!;

    private Inventory Inventory = null!;

    public int Film
    {
        set
        {
            Log.PrintLn(value);
            field = Math.Clamp(value, 0, maxFilm);
            FilmLabel.Text = $"{field}/{maxFilm}";
        }
        get;
    }
    public int maxFilm = 10;

    private bool aiming = false;

    public override async void _Ready()
    {
        Film = maxFilm;
        player = GetParent().GetParent<PlayerController>();
        photoTerminal ??= GetTree().CurrentScene.GetNodeOrNull<PhotoTerminal>("%PhotoTerminal");
        playerCamera = player.GetNode<CameraManager>().GetNode<Camera3D>()!;
        Inventory = GetParent<Inventory>();

        PhotoViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Disabled;
    }

    public override void _Input(InputEvent @event)
    {
        if (!CurrentItem)
            return;

        if (@event.IsActionPressed("look_cam"))
        {
            PhotoViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
            player.IsInMenu = true;
            aiming = true;
            Light.Visible = true;
        }

        if (@event.IsActionReleased("look_cam"))
        {
            PhotoViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Disabled;
            player.IsInMenu = false;
            aiming = false;
            Light.Visible = false;
        }

        if (@event.IsActionPressed("drop_item"))
        {
            var dropItem = MakeDropItem(Mesh.Mesh, Packed);
            dropItem.GlobalTransform = playerCamera.GlobalTransform;
            GetTree().CurrentScene.AddChild(dropItem);
            Inventory.RemoveCurrentItem();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!CurrentItem)
            return;

        if (aiming)
        {
            playerCamera.Fov = MathExt.Lerp(
                playerCamera.Fov,
                ViewfinderFov,
                (float)(ViewfinderLerp * delta)
            );
            PhotoLetterBox.Visible = true;
        }
        else
        {
            playerCamera.Fov = MathExt.Lerp(
                playerCamera.Fov,
                DefaultFov,
                (float)(ViewfinderLerp * delta)
            );
            PhotoLetterBox.Visible = false;
        }

        if (Input.IsActionJustPressed("take_photo") && aiming && Film > 0)
        {
            Film -= 1;
            var subjects = GetPhotoSubjects();
            Image image = GetViewportImage();
            PhotoData photo = PhotoData.New(Name, subjects, image.Data);
            Dictionary<string, PhotoGrade> grades = GetSubjectGrades(photo);
            FlashSFX();
            AddPhoto(photo, grades);
        }
        Mesh.GlobalTransform = playerCamera.GlobalTransform;
        Mesh.GlobalPosition += (-Mesh.GlobalBasis.Z / 2) + (Mesh.GlobalBasis.X / 2); //+ new Vector3(0, 0, 2);
        PhotoCameraCam.GlobalTransform = playerCamera.GlobalTransform;
    }

    private Dictionary<string, PhotoGrade> GetSubjectGrades(PhotoData photo)
    {
        Dictionary<string, PhotoGrade> result = [];

        foreach (var subjectName in photo.Subjects)
        {
            // TODO (j) where will fish be placed within the scene?
            var subject = GetTree().CurrentScene.GetNode<Node3D>(subjectName);

            // how centered the fish is
            var camToFish = subject.GlobalPosition - PhotoCameraCam.GlobalPosition;
            var camFacing = PhotoCameraCam.GlobalTransform.Basis.Z;
            var angle = camFacing.AngleTo(camToFish); // from a range of abt 2.7 - PI
            var angleScore = Math.Clamp((angle - 2.6) / (Math.PI - 2.6), 0, 1);

            //If the Fish is facing the camera
            var facingAngle = camFacing.AngleTo(-subject.GlobalTransform.Basis.Z); // from a range of 0 - PI
            var facingScore = Math.Clamp(facingAngle / Math.PI, 0, 1);

            // size of fish on the screen
            // distance from camera scaled based on the size of the bounding box
            var vis =
                subject.GetNode<Node3D>("shinfish").GetNode<MeshInstance3D>("%MeshInstance3D")
                as VisualInstance3D; // TODO(j) maybe have a "photoboundingbox" mesh for fish?
            var worldAabb = vis!.GetAabb() * vis.GlobalTransform;
            var sizeInPhoto = worldAabb.Volume / camToFish.Length(); // from a range of 0 - 0.1?
            var sizeScore = Math.Clamp(Math.Min(sizeInPhoto * 10, 1.0f), 0, 1);

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
        var img = PhotoViewport.GetTexture().GetImage();
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
        tween.Fn(() => FlashRect.Visible = true);
        tween.TweenInterval(.1);
        tween.Fn(() => FlashRect.Visible = false);
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
