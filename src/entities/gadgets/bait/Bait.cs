using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Bait : Area3D
{
    public override void _Notification(int what) => this.Notify(what);

    public static readonly PackedScene Packed = GD.Load<PackedScene>(
        "res://src/entities/gadgets/bait/bait.tscn"
    );

    [Export]
    float lifetime = 10;

    [Node]
    public required RayCast3D RayCast { set; get; }

    public bool ClearPathTo(Vec3 target)
    {
        RayCast.TargetPosition = target - GlobalPosition;
        RayCast.ForceRaycastUpdate();
        return RayCast.IsColliding();
    }

    public override void _Ready()
    {
        CreateTween().Fn(() => QueueFree(), lifetime);
    }

    public override void _PhysicsProcess(double delta)
    {
        //if(GetOverlappingBodies().Count )
        foreach (var body in GetOverlappingBodies())
        {
            var fish = body as Fish;
            if (fish!.getsBaited)
                fish.FoundBait(this);
        }
    }
}
