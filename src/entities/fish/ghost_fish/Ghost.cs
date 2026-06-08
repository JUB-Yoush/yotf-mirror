using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Ghost : Fish
{
    public override void _Notification(int what) => this.Notify(what);

    [Node]
    public required Node3D Skin { set; get; }

    [Export]
    public float speed = 2f;

    public override void _Ready()
    {
        base._Ready();
        Skin.Visible = false;
        PhotoCamera.AimingChanged += SetVisibility;
        navGraph = this.SceneRoot().GetNode<NavGraph>()!;
        CurrentNode = navGraph.RandomNode();
    }

    public override void _ExitTree()
    {
        PhotoCamera.AimingChanged -= SetVisibility;
    }

    void SetVisibility(bool state)
    {
        Skin.Visible = state;
    }

    public override void _Process(double delta)
    {
        if (SmoothMoveTo(CurrentNode!.GlobalPosition - GlobalPosition, speed, (float)delta))
        {
            CurrentNode = navGraph.RandomNode(CurrentNode);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();
    }
}
