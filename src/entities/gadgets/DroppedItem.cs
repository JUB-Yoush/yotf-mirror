using System;
using Godot;

namespace Yotf;

public partial class DroppedItem : RigidBody3D, IInteractable
{
    private Mesh mesh = null!;
    public PackedScene ItemRef = null!;
    private MeshInstance3D meshInstance = null!;

    public static readonly PackedScene Packed = GD.Load<PackedScene>("uid://btgb7l7cdigqw");

    //TODO(j) giving big macro/source generator potential...
    public DroppedItem Init(Mesh mesh, PackedScene packedItem)
    {
        this.mesh = mesh;
        this.ItemRef = packedItem;
        return this;
    }

    public override void _Ready()
    {
        meshInstance = GetNode<MeshInstance3D>("MeshInstance3D");
        meshInstance.Mesh = mesh;
    }

    public Item GivePickUpItem() => ItemRef.Instantiate<Item>();

    public void OnInteraction()
    {
        var player = GetTree().CurrentScene.GetNode<PlayerController>("Player");
        var inventory = player.GetNode<Inventory>("Inventory");

        if (inventory.GetEqippedItem() != null)
            return;
        inventory.AddItem(ItemRef.Instantiate<Item>());
        QueueFree();
    }

    public Mesh GetMesh() => mesh;
}
