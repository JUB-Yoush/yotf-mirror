using Godot;

public partial class CameraManager : Node3D
{
    private const float CameraMaxPitch = 70.0f * Mathf.Pi / 180.0f;
    private const float CameraMinPitch = -89.9f * Mathf.Pi / 180.0f;
    private const float CameraRatio = 0.625f;

    [Export] public float MouseSensitivity = 0.002f;
    [Export] public float MouseYInversion = -1.0f;

    public override void _Ready()
    {
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
    }

    public override void _Input(InputEvent pEvent)
    {
        if (pEvent is InputEventMouseMotion mouseMotion && Input.GetMouseMode() == Input.MouseModeEnum.Captured)
        {
            RotateCamera(mouseMotion.Relative);
            GetViewport().SetInputAsHandled();
        }
    }

    private void RotateCamera(Vector2 pRelative)
    {
        Vector3 rot = Rotation;
        rot.Y -= pRelative.X * MouseSensitivity;
        Rotation = rot;
        Orthonormalize();
        rot = Rotation;
        rot.X += pRelative.Y * MouseSensitivity * CameraRatio * MouseYInversion;
        rot.X = Mathf.Clamp(rot.X, CameraMinPitch, CameraMaxPitch);
        Rotation = rot;
    }
}
