using System;
using Godot;

namespace Yotf;

public partial class Item : Node3D, IDroppable
{
    public static readonly Texture2D DefaultTexture = GD.Load<Texture2D>(
        "res://assets/2d/mooninicon.png"
    );

    [Export]
    public string ItemName = "default_item_name";

    [Export]
    public Texture2D Icon = DefaultTexture;

    [Export]
    public Disposable.Restore restore = Disposable.Restore.None;

    public bool InInventory = false;
    public bool CurrentItem = false;

    public PackedScene PackedScene => throw new NotImplementedException();

    public Mesh DropMesh => throw new NotImplementedException();

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
