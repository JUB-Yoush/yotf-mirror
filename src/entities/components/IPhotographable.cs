using System;
using Godot;

namespace Yotf;

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
