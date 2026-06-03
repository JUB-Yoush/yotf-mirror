using System;
using Godot;

namespace Yotf;

public interface IPhotographable
{
    enum PhotoModifier
    {
        None,
        Ink,
    }

    public MeshInstance3D SubjectBoundingMesh { set; get; }
    public Node3D Subject { set; get; }
    public PhotoModifier Modifier
    {
        get => PhotoModifier.None;
    }
    public bool IsModifier
    {
        get => Modifier != PhotoModifier.None;
    }
    public bool IsInPhoto();
}
