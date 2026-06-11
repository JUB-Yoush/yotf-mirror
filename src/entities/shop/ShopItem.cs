using System;
using Godot;

namespace Yotf;

[GlobalClass]
public partial class ShopItem : Resource
{
    public enum Type
    {
        Upgrade,
        Item,
    }

    public enum Id
    {
        None,
        Camera,
        Flashlight,
        Sonar,
        Firecracker,
        Bait,
        Refill,
    }

    public enum Upgrade
    {
        None,
        Oxygen,
        Battery,
        Film,
        CameraZoom,
        SwimSpeed,
    }

    [Export]
    public Texture2D Icon = null!;

    [Export]
    public string Name = "unnamed";

    [Export]
    public string Description = "unnamed";

    [Export]
    public int Price = 0;

    [Export]
    public Type ItemType;

    [Export]
    public Id ItemId;

    [Export]
    public Upgrade upgrade;

    [Export]
    public Disposable.Restore restore;

    [Export]
    public float RestoreAmount;

    [Export]
    public PackedScene itemScene = null!;

    [Export]
    public Mesh DropMesh = null!;
}
