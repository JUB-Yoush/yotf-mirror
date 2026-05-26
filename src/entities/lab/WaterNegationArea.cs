using System;
using Godot;

namespace Yotf;

public partial class WaterNegationArea : Area3D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyExited(Node3D body)
    {
        var player = (PlayerController)body;
        player.SetState(player.SwimmingState);
    }

    private void OnBodyEntered(Node3D body)
    {
        var player = (PlayerController)body;
        player.SetState(player.WalkingState);
    }

    public override void _ExitTree()
    {
        BodyEntered -= OnBodyEntered;
        BodyExited -= OnBodyExited;
    }
}
