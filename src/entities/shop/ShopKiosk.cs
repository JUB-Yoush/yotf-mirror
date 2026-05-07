using System;
using Godot;

namespace Yotf;

public partial class ShopKiosk : Node3D, IInteractable
{
    Mesh mesh = null!;

    public override void _Ready()
    {
        mesh = GetNode<MeshInstance3D>("MeshInstance3D").Mesh;
    }

    public Mesh GetMesh() => mesh;

    public void OnInteraction()
    {
        throw new NotImplementedException();
    }
}
