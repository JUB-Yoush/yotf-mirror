using System;
using Godot;

namespace Yotf;

/// <summary>
/// Implemented on Node3Ds, Allows them to be captured by a Axolotl bubble.
/// Implementing bodies must exist on the Bubbleable layer.
/// </summary>
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
