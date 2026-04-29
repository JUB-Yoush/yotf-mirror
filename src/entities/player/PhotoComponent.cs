using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class PhotoComponent : Node
{
    const float DEFAULT_FOV = 90;
    const float VIEWFINDER_FOV = 50;
    const float VIEWFINDER_LERP = 20;

    [Export]
    private Camera3D _camera = null!;
    private TextureRect _photoLetterBox = null!;
    private ColorRect _flashRect = null!;
    private PhotoTerminal _photoTerminal = null!;
    private Control _netUi = null!;

    private PlayerController _player = null!;

    private bool _aiming = false;

    public override void _Ready()
    {
        _camera ??= GetNode<Camera3D>("%Camera3D");
        _photoLetterBox ??= GetTree().CurrentScene.GetNode<TextureRect>("%PhotoLetterBox");
        _flashRect ??= GetTree().CurrentScene.GetNode<ColorRect>("%FlashRect");
        _photoTerminal ??= GetTree().CurrentScene.GetNode<PhotoTerminal>("%PhotoTerminal");
        _netUi ??= GetTree().CurrentScene.GetNode<Control>("%NetUi");

        _player = GetParent<PlayerController>();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionPressed("look_cam"))
        {
            _aiming = true;
            _camera.Fov = MathExt.Lerp(
                _camera.Fov,
                VIEWFINDER_FOV,
                (float)(VIEWFINDER_LERP * delta)
            );
            _photoLetterBox.Visible = true;
        }
        else
        {
            _aiming = false;
            _camera.Fov = MathExt.Lerp(_camera.Fov, DEFAULT_FOV, (float)(VIEWFINDER_LERP * delta));
            _photoLetterBox.Visible = false;
        }

        if (Input.IsActionJustPressed("take_photo") && _aiming)
        {
            var subjects = GetPhotoSubjects();
            Image image = GetViewportImage();
            Photo photo = Photo.New(Name, subjects, image.Data);
            AddPhoto(photo.ToJson(), Name);
            ToggleUI(true);
        }
    }

    private void ToggleUI(bool state)
    {
        _photoLetterBox.Visible = state;
        _netUi.Visible = state;
    }

    //private Photo GetPhotoById(Guid photoId) => AllPhotos.First(x => x.Id == photoId);

    private Image GetViewportImage() => GetViewport().GetTexture().GetImage();

    private string[] GetPhotoSubjects()
    {
        List<string> result = [];
        foreach (var child in GetParent().GetChildren(true))
        {
            if (child is IPhotographable photographable && photographable.IsInPhoto())
            {
                result.Add(child.Name);
            }
        }
        return [.. result];
    }

    async void TakeScreenShot(string id)
    {
        //Whole camera system is quite hacky, we should use a subviewport for the camera viewfinder and put that in a a screenshot,
        _flashRect.Visible = true;
        await Task.Delay(100);
        _flashRect.Visible = false;
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

    [Rpc(
        MultiplayerApi.RpcMode.AnyPeer,
        CallLocal = true,
        TransferMode = MultiplayerPeer.TransferModeEnum.Reliable,
        TransferChannel = 0
    )]
    public void AddPhoto(string photoJson, string photoTaker)
    {
        var imgData = Photo.FromJson(photoJson);
        GD.Print(imgData.PhotoTaker, photoTaker);
        Rpc(MethodName.UpdateTerminalImage, photoJson);
        if (photoTaker == Name && _player.IsMultiplayerAuthority())
            Log.Print("I took this photo");
        else
            Log.Print("I didn't take this photo");
    }

    [Rpc(
        MultiplayerApi.RpcMode.AnyPeer,
        CallLocal = true,
        TransferMode = MultiplayerPeer.TransferModeEnum.Reliable,
        TransferChannel = 0
    )]
    public void UpdateTerminalImage(string photoJson)
    {
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
        _photoTerminal.GetNode<Sprite3D>("Sprite3D").Texture = imgTex;
    }
}
