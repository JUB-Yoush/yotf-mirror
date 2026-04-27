using Godot;

public partial class CameraManager : Node3D
{
    private const float CameraMaxPitch = 70.0f * Mathf.Pi / 180.0f;
    private const float CameraMinPitch = -89.9f * Mathf.Pi / 180.0f;
    private const float CameraRatio = 0.625f;

    [Export]
    public float MouseSensitivity = 0.002f;

    [Export]
    public float MouseYInversion = -1.0f;

    [Export]
    public float JoystickSensitivity = 15.0f;

    public override void _Ready()
    {
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
    }

    public override void _Process(double delta)
    {
        float x = Input.GetJoyAxis(0, JoyAxis.RightX);
        float y = Input.GetJoyAxis(0, JoyAxis.RightY);

        // Apply deadzone
        Vector2 joyInput = new(x, y);
        if (joyInput.LengthSquared() > 0.04f) // ~0.2 deadzone
            RotateCamera(joyInput * JoystickSensitivity * (float)delta * 100f);
    }

    public override void _Input(InputEvent pEvent)
    {
        if (
            pEvent is InputEventMouseMotion mouseMotion
            && Input.GetMouseMode() == Input.MouseModeEnum.Captured
        )
        {
            Log.Print(mouseMotion.Relative.ToString());
            RotateCamera(mouseMotion.Relative);
            GetViewport().SetInputAsHandled();
        }
    }

    private void RotateCamera(Vector2 pRelative)
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
}
