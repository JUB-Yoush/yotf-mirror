using System;
using Godot;

namespace Yotf;

// on any Node3D
public interface IBubbleable
{
    public Bubble? BubbleJail { set; get; }
    public Mesh Mesh { get; }
    public Node3D Self
    {
        get => (Node3D)this;
    }
    public bool CanBeBubbled
    {
        get => true;
    }
    public void PutInBubble();
    public void FreeFromBubble();
}
