using System;
using Godot;

namespace Yotf;

public interface IPhotographable
{
    public MeshInstance3D SubjectBoundingMesh { set; get; }
    public Node3D Subject { set; get; }
    public bool IsInPhoto();
}
