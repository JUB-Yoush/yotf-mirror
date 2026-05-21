using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class CameraManager : Node3D
{
    public override void _Notification(int what) => this.Notify(what);

    private const float CameraMaxPitch = 70.0f * Mathf.Pi / 180f;
    private const float CameraMinPitch = -89.9f * Mathf.Pi / 180f;
    private const float CameraRatio = 0.625f;

    [Export]
    public float MouseSensitivity = 0.002f;

    [Export]
    public float MouseYInversion = -1.0f;

    [Export]
    public float JoystickSensitivity = 15.0f;

    [Node]
    public required Camera3D Camera { set; get; }

    bool rolling = false;

    public override void _Ready()
    {
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
    }

    public override void _Process(double delta)
    {
        float x = Input.GetJoyAxis(0, JoyAxis.RightX);
        float y = Input.GetJoyAxis(0, JoyAxis.RightY);
        rolling = Input.IsActionPressed("roll");

        // Apply deadzone
        Vector2 joyInput = new(x, y);
        if (joyInput.LengthSquared() > 0.04f) // ~0.2 deadzone
            RotateCameraFree(joyInput * JoystickSensitivity * (float)delta * 100f);
    }

    public override void _Input(InputEvent pEvent)
    {
        if (
            pEvent is InputEventMouseMotion mouseMotion
            && Input.GetMouseMode() == Input.MouseModeEnum.Captured
        )
        {
            if (rolling)
            {
                RollCamera(mouseMotion.Relative);
            }
            else
            {
                RotateCameraFree(mouseMotion.Relative);
            }
            GetViewport().SetInputAsHandled();
        }
    }

    private void RollCamera(Vector2 mouseMotion)
    {
        Rotation = Rotation with
        {
            Z = Rotation.Z + mouseMotion.X * MouseSensitivity * CameraRatio * MouseYInversion,
        };
    }

    private void RotateCameraClamped(Vector2 pRelative)
    {
        Rotation = Rotation with { Y = Rotation.Y - pRelative.X * MouseSensitivity };
        Orthonormalize();
        Rotation = Rotation with
        {
            X = Mathf.Clamp(
                Rotation.X + pRelative.Y * MouseSensitivity * CameraRatio * MouseYInversion,
                CameraMinPitch,
                CameraMaxPitch
            ),
        };
    }

    private void RotateCameraFree(Vector2 mouseMotion)
    {
        var upSign = Mathf.Sign(Camera.GlobalTransform.Basis.Y.Y);
        if (upSign == 0)
            upSign = 1;
        Rotation = Rotation with { Y = Rotation.Y - mouseMotion.X * MouseSensitivity * upSign };
        //Orthonormalize();
        Camera.Rotation = Camera.Rotation with
        {
            X =
                Camera.Rotation.X
                + mouseMotion.Y * MouseSensitivity * CameraRatio * MouseYInversion,
        };
    }
}
