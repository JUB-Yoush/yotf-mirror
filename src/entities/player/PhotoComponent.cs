using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    private Camera3D _photoCamera = null!;
    private SubViewport _photoViewport = null!;

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
        _photoCamera = GetNode<Camera3D>("SubViewport/Camera3D");
        _photoViewport = GetNode<SubViewport>("SubViewport");
        _photoViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Disabled;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("look_cam"))
        {
            _photoViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
            _aiming = true;
        }

        if (@event.IsActionReleased("look_cam"))
        {
            _photoViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Disabled;
            _aiming = false;
        }

        if (@event.IsActionPressed("take_photo") && _aiming)
        {
            var subjects = GetPhotoSubjects();

            Image image = GetViewportImage();
            Photo photo = Photo.New(Name, subjects, image.Data);
            PhotoGrade grade = EvaluatePhoto(photo);
            AddPhoto(photo.ToJson(), Name);
            FlashSFX();
        }
    }

    private PhotoGrade EvaluatePhoto(Photo photo)
    {
        foreach (var subjectName in photo.Subjects)
        {
            // TODO (j) where will fish be placed within the scene?
            var subject = GetTree().CurrentScene.GetNode<Node3D>(subjectName);

            // how centered the fish is
            var camToFish = subject.GlobalPosition - _photoCamera.GlobalPosition;
            var camFacing = _photoCamera.GlobalTransform.Basis.Z;
            var angle = camFacing.AngleTo(camToFish); // from a range of abt 2.7 - PI
            var angleScore = (angle - 2.6) / (Math.PI - 2.6);

            //If the Fish is facing the camera
            var facingAngle = camFacing.AngleTo(-subject.GlobalTransform.Basis.Z); // from a range of 0 - PI
            var facingScore = (facingAngle / Math.PI);

            // size of fish on the screen
            // Get bounding box, project to camera view, calculate area/size of screen
            var vis = subject.GetNode<MeshInstance3D>("MeshInstance3D") as VisualInstance3D;
            var world_aabb = vis!.GetAabb() * vis.GlobalTransform;

            // project all 8 aabb points to the screen and find the smallest rectangle that fits all of them, divide the area of that rect with the area of the screen
            Vector2 minPos = new(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 maxPos = new(float.NegativeInfinity, float.NegativeInfinity);

            for (int i = 0; i < 8; i++)
            {
                var corner = world_aabb.GetEndpoint(i);
                if (_photoCamera.IsPositionBehind(corner))
                {
                    GD.Print($"point {i} behind");
                    continue;
                }

                var screenPos = _photoCamera.UnprojectPosition(corner);
                GD.Print($"corner{i} {screenPos}");
                minPos = minPos.Min(screenPos);
                maxPos = maxPos.Max(screenPos);
            }
            var screenBoundingBox = new Rect2(minPos, maxPos - minPos);
            GD.Print(screenBoundingBox.Area);
        }
        return new PhotoGrade();
    }

    public override void _PhysicsProcess(double delta)
    {
        _photoCamera.GlobalTransform = _camera.GlobalTransform;
        if (_aiming)
        {
            _camera.Fov = MathExt.Lerp(
                _camera.Fov,
                VIEWFINDER_FOV,
                (float)(VIEWFINDER_LERP * delta)
            );
            _photoLetterBox.Visible = true;
        }
        else
        {
            _camera.Fov = MathExt.Lerp(_camera.Fov, DEFAULT_FOV, (float)(VIEWFINDER_LERP * delta));
            _photoLetterBox.Visible = false;
        }
    }

    private Image GetViewportImage()
    {
        var img = _photoViewport.GetTexture().GetImage();
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
        tween.Call(() => _flashRect.Visible = true);
        tween.TweenInterval(.1);
        tween.Call(() => _flashRect.Visible = false);
    }

    void TakeScreenShot(string id)
    {
        GetViewport().GetTexture().GetImage().SavePng($"user://live-camera-roll/{id}.png");
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
