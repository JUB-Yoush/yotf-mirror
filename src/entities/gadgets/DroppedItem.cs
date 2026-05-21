using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class DroppedItem : RigidBody3D, IInteractable, IOnMiniMap
{
    public override void _Notification(int what) => this.Notify(what);

    private Mesh meshData = null!;
    public PackedScene ItemRef = null!;

    [Node]
    MeshInstance3D Mesh { set; get; }

    public static DroppedItem New(Mesh mesh, PackedScene packedItem)
    {
        var dropped = Packed.Instantiate<DroppedItem>();
        dropped.meshData = mesh;
        dropped.ItemRef = packedItem;
        return dropped;
    }

    public static readonly PackedScene Packed = GD.Load<PackedScene>("uid://btgb7l7cdigqw");

    public required Mesh InteractionMesh
    {
        get => meshData;
        set;
    }

    public override void _Ready()
    {
        Mesh.Mesh = meshData;
    }

    public Item GivePickUpItem() => ItemRef.Instantiate<Item>();

    public void OnInteraction()
    {
        // var player = GetTree().CurrentScene.GetNode<PlayerController>("Player");
        // var inventory = player.GetNode<Inventory>("Inventory");
        var player = GetTree().CurrentScene.GetNode<PlayerController>();
        var inventory = player.GetNode<Inventory>()!;

        if (inventory.GetEqippedItem() != null)
            return;
        inventory.AddItem(ItemRef.Instantiate<Item>());
        QueueFree();
    }
}
