using System;
using Godot;

public interface IPhotographable
{
    public bool IsInPhoto();
    public Node3D GetSubject();
}
