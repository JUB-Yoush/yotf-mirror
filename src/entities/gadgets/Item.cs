using System;
using Godot;

public partial class Item : Node3D
{
    public enum ItemState
    {
        InInventory,
        OnGround,
    }

    public static readonly Texture2D moonin = GD.Load<Texture2D>("res://assets/2d/mooninicon.png");
    public ItemState itemState = ItemState.OnGround;
    public string ItemName = "default_item_name";
    public Texture2D Icon = moonin;
    public bool inInventory = false;
    public bool currentItem = false;

    public virtual void Enter() { }

    public virtual void Exit() { }
}
