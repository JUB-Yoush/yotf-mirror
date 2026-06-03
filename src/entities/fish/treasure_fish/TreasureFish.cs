using System;
using System.Threading.Tasks;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class TreasureFish : Fish, IPhotographable
{
    public override void _Notification(int what) => this.Notify(what);

    public new IPhotographable.PhotoModifier Modifier
    {
        get => IPhotographable.PhotoModifier.Treasure;
        set;
    }

    [Export]
    float WaitTimer = 1f;

    [Export]
    float speed = 10f;

    [Export]
    float lifetime = 15f;

    bool IPhotographable.IsModifier
    {
        get => true;
    }

    public const int Value = 2;

    bool swimmingUp = false;

    public override void _Ready()
    {
        var tween = CreateTween();
        tween.TweenFn<Vector3>(
            (target) => LookAt(GlobalPosition - target),
            -GlobalTransform.Basis.Z,
            Vector3.Up,
            1
        );
        tween.Fn(() => swimmingUp = true, WaitTimer, true);
        tween.Fn(() => QueueFree(), lifetime, true);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (swimmingUp)
        {
            Velocity = Vector3.Up * speed;
            MoveAndSlide();
        }
    }
}
