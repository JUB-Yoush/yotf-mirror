using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;

public partial class Player : CharacterBody3D
{
    float LookSensitivity = 0.006f;
    const float DEFAULT_FOV = 90;
    const float VIEWFINDER_FOV = 50;
    const float VIEWFINDER_LERP = 20;
    public Camera3D Camera = null!;
    Node3D PhotoHead = null!;
    Node3D PhotoBody = null!;
    Vector3 WishDir;
    Vector3 targetVelocity = Vector3.Zero;
    TextureRect PhotoLetterBox = null!;
    ColorRect FlashRect = null!;
    bool aiming = false;
    List<Photo> AllPhotos = [];
    List<Photo> LocalPhotos = [];

    public override void _Ready()
    {
        PhotoLetterBox = GetNode<TextureRect>("../PhotoLetterBox");
        FlashRect = GetNode<ColorRect>("../FlashRect");
        Camera = GetNode<Camera3D>("Head/Camera3D");
        if (IsMultiplayerAuthority())
        {
            Camera.Current = true;
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(int.Parse(Name));
    }

    public override void _Input(InputEvent @event)
    {
        if (!IsMultiplayerAuthority())
            return;
        if (
            Input.MouseMode == Input.MouseModeEnum.Captured
            && @event is InputEventMouseMotion mouseMotionEvent
        )
        {
            RotateY(-mouseMotionEvent.Relative.X * LookSensitivity);
            Camera.RotateX(-mouseMotionEvent.Relative.Y * LookSensitivity);

            Camera.Rotation = Camera.Rotation with
            {
                X = Mathf.Clamp(Camera.Rotation.X, Mathf.DegToRad(-90), Mathf.DegToRad(90)),
            };
        }
    }

    [Rpc(
        MultiplayerApi.RpcMode.AnyPeer,
        CallLocal = true,
        TransferMode = MultiplayerPeer.TransferModeEnum.Reliable,
        TransferChannel = 0
    )]
    void AddPhoto(string photoJson, string photoTaker)
    {
        var imgData = Photo.FromJson(photoJson);
        GD.Print(imgData.PhotoTaker, photoTaker);
        Rpc(MethodName.UpdateTerminalImage, photoJson);
        if (photoTaker == Name && IsMultiplayerAuthority())
        {
            Log.Print("I took this photo");
        }
        else
        {
            Log.Print("I didn't take this photo");
        }

        imgData.Subjects.ToList().ForEach(Log.Print);
    }

    [Rpc(
        MultiplayerApi.RpcMode.AnyPeer,
        CallLocal = true,
        TransferMode = MultiplayerPeer.TransferModeEnum.Reliable,
        TransferChannel = 0
    )]
    void UpdateTerminalImage(string photoJson)
    {
        var term = GetNode<PhotoTerminal>("../PhotoTerminal");

        Photo imgData = Photo.FromJson(photoJson);
        var photoImg = Image.CreateFromData(
            imgData.Width,
            imgData.Height,
            imgData.Mipmaps,
            Image.Format.Rgb8,
            imgData.Data
        );
        var imgTex = new ImageTexture();
        imgTex.SetImage(photoImg);
        var sprite = term.GetNode<Sprite3D>("Sprite3D");
        sprite.Texture = imgTex;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsMultiplayerAuthority())
            return;

        if (Input.IsActionJustPressed("interact"))
        {
            //Rpc(MethodName.UpdateTerminalImage);
            //UpdateTerminalImage(AllPhotos[^1].Id);
        }

        if (Input.IsActionPressed("look_cam"))
        {
            aiming = true;
            Camera.Fov = MathExt.Lerp(Camera.Fov, VIEWFINDER_FOV, (float)(VIEWFINDER_LERP * delta));
            PhotoLetterBox.Visible = true;
        }
        else
        {
            aiming = false;
            Camera.Fov = MathExt.Lerp(Camera.Fov, DEFAULT_FOV, (float)(VIEWFINDER_LERP * delta));
            PhotoLetterBox.Visible = false;
        }

        if (Input.IsActionJustPressed("take_photo") && aiming)
        {
            var subjects = GetPhotoSubjectNames();
            Image image = GetViewportImage();
            Photo photo = Photo.New(Name, subjects, image.Data);
            Rpc(MethodName.AddPhoto, photo.ToJson(), Name);
            ToggleUI(true);
        }

        Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
        WishDir = GlobalTransform.Basis * new Vector3(inputDir.X, 0, inputDir.Y);
        targetVelocity = WishDir.Normalized() * 10;

        //TODO (j) mouse capturing is weird over the network, this is a workaround
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        if (Input.IsActionJustPressed("capture_mouse"))
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
        Velocity = targetVelocity;
        MoveAndSlide();
    }

    private void ToggleUI(bool state)
    {
        PhotoLetterBox.Visible = state;
        GetNode<Control>("../NetUi").Visible = state;
    }

    //private Photo GetPhotoById(Guid photoId) => AllPhotos.First(x => x.Id == photoId);

    private Image GetViewportImage() => GetViewport().GetTexture().GetImage();

    private Node3D[] GetPhotoSubjects() =>
        GetParent()
            .GetChildren(true)
            .OfType<IPhotographable>()
            .Where(x => x.IsInPhoto())
            .Select(x => x.GetSubject())
            .ToArray();

    private string[] GetPhotoSubjectNames() =>
        GetParent()
            .GetChildren(true)
            .OfType<IPhotographable>()
            .Where(x => x.IsInPhoto())
            .Select(x => (string)x.GetSubject().Name)
            .ToArray();

    async void TakeScreenShot(string id)
    {
        //Whole camera system is quite hacky, we should use a subviewport for the camera viewfinder and put that in a a screenshot,
        FlashRect.Visible = true;
        await Task.Delay(100);
        FlashRect.Visible = false;
        await Task.Delay(50);
        TryMakeDir("user://live-camera-roll");
        GetViewport()
            .GetTexture()
            .GetImage()
            .SavePng($"user://live-camera-roll/{id.ToString()}.png");
    }

    private void TryMakeDir(string path)
    {
        using var dir = DirAccess.Open(path);
        if (dir == null)
        {
            DirAccess.MakeDirAbsolute(path);
        }
    }

    //TODO (j) why no work
}
