using System;
using Godot;

namespace Yotf;

// areas that implement this interface must be on collision layer 3
public interface IInteractable
{
    public static readonly Material Outline = GD.Load<ShaderMaterial>("uid://cgcywsatye4gv");
    public void OnInteraction();
    public Mesh GetMesh();
    public void OutlineMesh()
    {
        var mesh = GetMesh();
        mesh.SurfaceGetMaterial(0).NextPass = Outline;
    }

    public void RemoveOutlineMesh()
    {
        var mesh = GetMesh();
        mesh.SurfaceGetMaterial(0).NextPass = null;
    }
}
