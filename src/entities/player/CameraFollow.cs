using Godot;

namespace Yotf;

public partial class CameraFollow : Camera3D
{
    [Export]
    public float LerpPower = 5.0f;

    private Node3D armPosition = null!;

    public override void _Ready()
    {
        armPosition = GetNode<Node3D>("../Arm/ArmPosition");
    }

    public override void _Process(double delta)
    {
        Position = Position.Lerp(armPosition.Position, (float)delta * LerpPower);
    }
}
