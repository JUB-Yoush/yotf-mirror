using System;
using Godot;

namespace Yotf;

// on any Node3D
public interface IBubbleable
{
    public Bubble? BubbleJail { set; get; }
    public Mesh Mesh { get; }
    public Node3D Spatial
    {
        get => (Node3D)this;
    }
    public bool CanBeBubbled
    {
        get => true;
    }
    public Vector3 GlobalPosition
    {
        get => Spatial.GlobalPosition;
    }
    public bool AxolotlTargets
    {
        get => true;
    }
    public void PutInBubble();
    public void FreeFromBubble();
}
