using System;
using Godot;
using Yotf;

namespace Yotf;

// TODO (j) should we make another interface for things that can go in chests?
[Meta(typeof(IAutoNode))]
public partial class Chest : Node3D, IInteractable
{
    public override void _Notification(int what) => this.Notify(what);

    [Export]
    public PackedScene InsideChest = null!;

    [Node]
    public required MeshInstance3D Mesh { set; get; }

    Mesh IInteractable.InteractionMesh
    {
        get => Mesh.Mesh;
    }

    public void OnInteraction()
    {
        var node = InsideChest.Instantiate();
        this.SceneRoot().AddChild(node);
        QueueFree();
    }
}
