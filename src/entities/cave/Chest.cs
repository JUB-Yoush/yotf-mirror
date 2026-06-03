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
    public PackedScene? InsideChest = null!;

    [Node]
    public required MeshInstance3D Mesh { set; get; }

    [Node]
    public required CollisionShape3D InteractionShape { set; get; }

    Mesh IInteractable.InteractionMesh
    {
        get => Mesh.Mesh;
    }

    public void OnInteraction()
    {
        if (InsideChest != null)
        {
            var node = InsideChest.Instantiate<Node3D>();
            this.SceneRoot().AddChild(node);
            node.GlobalPosition = GlobalPosition + Vector3.Up;
        }

        Mesh.Visible = false;
        InteractionShape.Disabled = true;
    }
}
