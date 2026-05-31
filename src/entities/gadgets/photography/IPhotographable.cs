using System;
using Godot;

namespace Yotf;

public interface IPhotographable
{
    public MeshInstance3D SubjectBoundingMesh { set; get; }
    public bool IsInPhoto();
    public Node3D GetSubject();
}
