using System;
using Godot;

namespace Yotf;

[GlobalClass]
public partial class ShopItem : Resource
{
    public enum ItemType
    {
        Upgrade,
        Item,
    }

    public enum Item
    {
        None,
        Camera,
        Flashlight,
    }

    public enum Upgrade
    {
        None,
        Oxygen,
        Battery,
    }

    [Export]
    public Texture2D Icon = null!;

    [Export]
    public string ItemName = "unnamed";

    [Export]
    public int Price = 0;

    [Export]
    public ItemType itemType;

    [Export]
    public Item item;

    [Export]
    public Upgrade upgrade;
}
