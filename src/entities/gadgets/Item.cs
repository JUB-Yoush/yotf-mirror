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

    public static readonly PackedScene Packed = GD.Load<PackedScene>("uid://dn7sjuk12ci0e");

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

    public static DroppedItem MakeDropItem(Mesh mesh, PackedScene itemPacked) =>
        DroppedItem.New(mesh, itemPacked);
}
