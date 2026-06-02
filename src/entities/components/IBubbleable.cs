using System;
using Godot;

namespace Yotf;

// on any Node3D
public interface IBubbleable
{
    public Bubble? BubbleJail { set; get; }
    public Mesh Mesh { get; }
    public float MeshScale
    {
        get => 1;
    }
    public Node3D Spatial
    {
        get => (Node3D)this;
    }
    public bool CanBeBubbled
    {
        get => true;
    }
    public bool AxolotlTargets
    {
        get => true;
    }
    public void PutInBubble();
    public void FreeFromBubble();
}
