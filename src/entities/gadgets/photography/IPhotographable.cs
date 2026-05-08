using System;
using Godot;

namespace Yotf;

public interface IPhotographable
{
    public bool IsInPhoto();
    public Node3D GetSubject();
}
