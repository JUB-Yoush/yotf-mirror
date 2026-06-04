using System;
using Godot;

namespace Yotf;

/// <summary>
/// Allows Node3D to hear noises.
/// Node must have child Area on collision layer Interactable
/// InteractionMesh must have a material applied.
/// </summary>
public interface IInteractable
{
    public static readonly Material Outline = GD.Load<ShaderMaterial>(
        "res://assets/materials/outline_material.tres"
    );

    public void OnInteraction();
    public Mesh InteractionMesh { get; }
    public bool CanInteract
    {
        get => true;
    }
    public void OutlineMesh()
    {
        var mesh = InteractionMesh;
        mesh.SurfaceGetMaterial(0).NextPass = Outline;
    }

    public void RemoveOutlineMesh()
    {
        var mesh = InteractionMesh;
        mesh.SurfaceGetMaterial(0).NextPass = null;
    }
}
