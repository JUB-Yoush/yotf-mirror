using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class PhotoCamera : Item, IMakeNoise, IDroppable
{
    public override void _Notification(int what) => this.Notify(what);

    private static readonly Texture2D moonin = GD.Load<Texture2D>("res://assets/2d/mooninicon.png");

    public static readonly PackedScene Packed = GD.Load<PackedScene>(
        "res://src/entities/gadgets/photography/photo_camera.tscn"
    );

    public static Action<bool>? AimingChanged;

    public List<Photo> Photos = [];

    private bool equipped = false;
    const float DefaultFov = 90;
    const float ViewfinderLerp = 20;
    const float DefaultViewfinderFov = 70;
    private float ViewfinderFov = DefaultViewfinderFov;

    static readonly SubViewport.UpdateMode[] updateModes =
    [
        SubViewport.UpdateMode.Disabled,
        SubViewport.UpdateMode.Always,
    ];

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

    [Node]
    public required AudioStreamPlayer3D AudioStreamPlayer { get; set; }

    public AudioStreamPlayer3D NoiseSource
    {
        get => AudioStreamPlayer;
    }

    private Player player = null!;

    private Camera3D playerCamera = null!;

    private PhotoTerminal? photoTerminal = null!;

    private Inventory Inventory = null!;

    public int Film
    {
        set
        {
            field = Math.Clamp(value, 0, PlayerStats.MaxFilm);
            FilmLabel.Text = $"{Film}/{PlayerStats.MaxFilm}";
        }
        get;
    }

    public new PackedScene PackedScene => Packed;

    public new Mesh DropMesh => Mesh.Mesh;

    private bool Aiming
    {
        get;
        set
        {
            field = value;
            AimingChanged?.Invoke(field);
        }
    }

    public override void _Ready()
    {
        player = GetParent().GetParent<Player>();
        Film = PlayerStats.MaxFilm;
        playerCamera = player.GetNode<CameraManager>().GetNode<Camera3D>()!;
        Inventory = GetParent<Inventory>();
        Lab.CurrentLabUpdated += CurrentLabUpdated;
        photoTerminal = Lab.CurrentLab!.PhotoTerminal;

        PhotoViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Disabled;
    }

    public override void _ExitTree()
    {
        Lab.CurrentLabUpdated -= CurrentLabUpdated;
    }

    public override void Equipped()
    {
        FilmLabel.Text = $"{Film}/{PlayerStats.MaxFilm}";
    }

    public override void Added()
    {
        Film = PlayerStats.MaxFilm;
    }

    public override void Removed()
    {
        CreateTween().LerpProperty(playerCamera, Camera3D.PropertyName.Fov, DefaultFov, .3f);
        ToggleCameraAim(false);
    }

    public void CurrentLabUpdated(Lab newLab)
    {
        photoTerminal = newLab.PhotoTerminal;
    }

    public override void _Input(InputEvent @event)
    {
        if (!CurrentItem)
            return;

        if (@event.IsActionPressed("scroll_up"))
            ViewfinderFov = Math.Max(
                ViewfinderFov - 2,
                Math.Max(DefaultViewfinderFov - PlayerStats.MaxZoom, 10)
            );

        if (@event.IsActionPressed("scroll_down"))
            ViewfinderFov = Math.Min(ViewfinderFov + 2, 90);

        if (@event.IsActionPressed("look_cam"))
        {
            PhotoViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
            player.IsLookingInCamera = true;
            Aiming = true;
            Light.Visible = true;
            ViewfinderFov = DefaultViewfinderFov;
        }

        if (@event.IsActionReleased("look_cam"))
        {
            PhotoViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Disabled;
            player.IsLookingInCamera = false;
            Aiming = false;
            Light.Visible = false;
            ViewfinderFov = DefaultViewfinderFov;
        }

        if (@event.IsActionPressed("drop_item"))
        {
            var dropItem = IDroppable.MakeDropItem(this);
            dropItem.GlobalTransform = playerCamera.GlobalTransform;
            GetTree().CurrentScene.AddChild(dropItem);
            Inventory.RemoveCurrentItem();
        }
    }

    public override void _Process(double delta)
    {
        Mesh.GlobalTransform = playerCamera.GlobalTransform;
        Mesh.GlobalPosition += (-Mesh.GlobalBasis.Z / 2) + (Mesh.GlobalBasis.X / 2); //+ new Vec3(0, 0, 2);
        PhotoCameraCam.GlobalTransform = playerCamera.GlobalTransform;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!CurrentItem)
            return;

        if (Aiming)
        {
            playerCamera.Fov = Mathf.Lerp(
                playerCamera.Fov,
                ViewfinderFov,
                (float)(ViewfinderLerp * delta)
            );
            PhotoLetterBox.Visible = true;
        }
        else
        {
            playerCamera.Fov = Mathf.Lerp(
                playerCamera.Fov,
                DefaultFov,
                (float)(ViewfinderLerp * delta)
            );
            PhotoLetterBox.Visible = false;
        }

        if (Input.IsActionJustPressed("take_photo") && Aiming && Film > 0)
        {
            TakePhoto();
        }
    }

    void TakePhoto()
    {
        Film -= 1;
        var subjects = GetPhotoSubjects();
        var modifiers = GetPhotoModifiers();
        Image image = GetViewportImage();
        PhotoData photo = PhotoData.New(Name, subjects, image.Data);
        Dictionary<string, PhotoGrade> grades = GetSubjectGrades(photo, modifiers);
        FlashSFX();
        IMakeNoise.MakeNoise(this, 5, Sfx.CameraShutter, 5);
        AddPhoto(photo, grades, modifiers);
    }

    private void ToggleCameraAim(bool state)
    {
        PhotoViewport.RenderTargetUpdateMode = updateModes[Convert.ToInt32(state)];
        player.IsLookingInCamera = state;
        Aiming = state;
        Light.Visible = state;
    }

    private Dictionary<string, PhotoGrade> GetSubjectGrades(
        PhotoData photo,
        IPhotographable.PhotoModifier[] modifiers
    )
    {
        Dictionary<string, PhotoGrade> result = [];
        var modSet = modifiers.ToHashSet<IPhotographable.PhotoModifier>();

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
            var photographable = subject as IPhotographable;
            var vis = photographable.SubjectBoundingMesh as VisualInstance3D;
            var worldAabb = vis!.GetAabb() * vis.GlobalTransform;
            var sizeInPhoto = worldAabb.Volume / camToFish.Length(); // from a range of 0 - 0.1?
            var sizeScore = Math.Clamp(sizeInPhoto / 100, 0, 1);
            var inAction = subject is IDoesAction actionable && actionable.InAction;

            //fish lighting
            // TODO (j) implement
            var lightScore = 1f;
            result.Add(
                subject.Name,
                new(
                    (float)angleScore,
                    sizeScore,
                    (float)facingScore,
                    lightScore,
                    photo.Subjects.Length,
                    inAction,
                    modSet.Contains(IPhotographable.PhotoModifier.Ink),
                    false // fish can't die (yet)
                )
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
        foreach (var photographable in GetTree().CurrentScene.GetNodes<IPhotographable>(true))
        {
            if (photographable.IsInPhoto() && !photographable.IsModifier)
                result.Add(photographable.Subject.Name);
        }
        return [.. result];
    }

    private IPhotographable.PhotoModifier[] GetPhotoModifiers()
    {
        List<IPhotographable.PhotoModifier> result = [];

        foreach (var photographable in GetTree().CurrentScene.GetNodes<IPhotographable>(true))
            if (photographable.IsInPhoto() && photographable.IsModifier)
            {
                if (photographable.Modifier == IPhotographable.PhotoModifier.Treasure)
                {
                    player.Stats.Money += TreasureFish.Value;
                }
                else
                {
                    result.Add(photographable.Modifier);
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

    public void AddPhoto(
        PhotoData photoData,
        Dictionary<string, PhotoGrade> grades,
        IPhotographable.PhotoModifier[] modifiers
    )
    {
        var photo = new Photo(photoData, grades, modifiers);
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
        photoTerminal!.GetNode<Sprite3D>("Sprite3D").Texture = imgTex;
    }

    public void ClearPhotos()
    {
        Photos = [];
    }
}
