using System;
using Godot;

namespace Yotf;

public partial class DummyFish : Area3D, IPhotographable
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
