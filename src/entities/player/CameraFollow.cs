using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class CameraFollow : Camera3D
{
    public override void _Notification(int what) => this.Notify(what);

    [Export]
    public float LerpPower = 5.0f;

    [Node("../Arm/ArmPosition")]
    public required Node3D ArmPosition { set; get; }

    public override void _Ready()
    {
        ArmPosition = GetNode<Node3D>("../Arm/ArmPosition");
    }

    public override void _Process(double delta)
    {
        Position = Position.Lerp(ArmPosition.Position, (float)delta * LerpPower);
    }
}
