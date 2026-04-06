using Godot;

public partial class CameraFollow : Camera3D
{
    [Export]
    public float LerpPower = 5.0f;

    private Node3D _armPosition;

    public override void _Ready()
    {
        _armPosition = GetNode<Node3D>("../Arm/ArmPosition");
    }

    public override void _Process(double delta)
    {
        Position = Position.Lerp(_armPosition.Position, (float)delta * LerpPower);
    }
}
