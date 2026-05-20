using System;
using Godot;

namespace Yotf;

public partial class Item : Node3D
{
    public static readonly Texture2D DefaultTexture = GD.Load<Texture2D>(
        "res://assets/2d/mooninicon.png"
    );

    public static DroppedItem MakeDropItem(Mesh mesh, PackedScene itemPacked) =>
        DroppedItem.New(mesh, itemPacked);

    [Export]
    public string ItemName = "default_item_name";

    [Export]
    public Texture2D Icon = DefaultTexture;

    [Export]
    public Mesh DropMesh = null!;

    public bool InInventory = false;
    public bool CurrentItem = false;

    public virtual void Added() { }

    public virtual void Removed() { }

    public virtual void Equipped()
    {
        Visible = true;
    }

    public virtual void Unequipped()
    {
        Visible = false;
    }
}
