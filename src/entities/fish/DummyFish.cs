using System;
using Godot;

public partial class DummyFish : Node3D, IPhotographable
{
    public Node3D GetSubject()
    {
        return this;
    }

    public bool IsInPhoto()
    {
        return GetNode<VisibleOnScreenNotifier3D>("VisibleOnScreenNotifier3D").IsOnScreen();
    }
}
