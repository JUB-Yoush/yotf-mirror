using System;
using Godot;

namespace Yotf;

/// <summary>
/// Node3Ds that implement this interface can appear in photos.
/// </summary>
public interface IPhotographable
{
    enum PhotoModifier
    {
        None,
        Ink,
        Treasure,
    }

    public MeshInstance3D SubjectBoundingMesh { set; get; }
    public Node3D Subject
    {
        get => (Node3D)this;
    }
    public PhotoModifier Modifier { get; set; }
    public bool IsModifier
    {
        get => Modifier != PhotoModifier.None;
    }
    public bool IsInPhoto();
}
