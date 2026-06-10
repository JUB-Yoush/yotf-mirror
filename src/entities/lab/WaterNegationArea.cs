using System;
using Godot;

namespace Yotf;

/// <summary>
/// Nullifies a WaterVolume if placed within
/// Used to remove the water from underwater labs.
/// </summary>
public partial class WaterNegationArea : Area3D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyExited(Node3D body)
    {
        var player = (Player)body;
        player.InNegationArea = false;
        player.SetState(player.SwimmingState);
    }

    private void OnBodyEntered(Node3D body)
    {
        var player = (Player)body;
        player.InNegationArea = true;
        player.SetState(player.WalkingState);
    }

    public override void _ExitTree()
    {
        BodyEntered -= OnBodyEntered;
        BodyExited -= OnBodyExited;
    }
}
