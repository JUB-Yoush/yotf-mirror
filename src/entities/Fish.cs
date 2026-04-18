using System;
using Godot;

public partial class Fish : Node3D, IPhotographable
{
    public VisibleOnScreenNotifier3D VisibilityNotif = null!;

    public override void _Ready()
    {
        VisibilityNotif = GetNode<VisibleOnScreenNotifier3D>("VisibleOnScreenNotifier3D");
    }

    public bool IsInPhoto()
    {
        return VisibilityNotif.IsOnScreen();
    }

    public Node3D GetSubject()
    {
        return this;
    }
}
