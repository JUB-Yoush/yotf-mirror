using System;
using Godot;

namespace Yotf;

public partial class Item : Node3D
{
    public enum State
    {
        InInventory,
        OnGround,
    }

    public static readonly Texture2D DefaultTexture = GD.Load<Texture2D>(
        "res://assets/2d/mooninicon.png"
    );

    private static readonly PackedScene Packed = GD.Load<PackedScene>(
        "res://src/entities/gadgets/item.tscn"
    );

    public State ItemState = State.OnGround;

    [Export]
    public string ItemName = "default_item_name";

    [Export]
    public Texture2D Icon = DefaultTexture;

    [Export]
    public Mesh DropMesh = null!;

    public bool InInventory = false;
    public bool CurrentItem = false;

    public virtual void Enter() { }

    public virtual void Exit() { }

    // TODO(j) this JUST gives you the single node, you need to instance the entire scene.
    // public virtual DroppedItem MakeDropItem() =>
    //     Packed.Instantiate<DroppedItem>().Init(DropMesh, Packed);

    public static DroppedItem MakeDropItem(Mesh mesh, PackedScene itemPacked) =>
        DroppedItem.Packed.Instantiate<DroppedItem>().Init(mesh, itemPacked);
}
