using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class ShopKiosk : Node3D, IInteractable
{
    public override void _Notification(int what) => this.Notify(what);

    const string shopItemPath = "res://assets/data/shop/items";
    const string shopUpgradePath = "res://assets/data/shop/upgrades";

    private static readonly PackedScene shopUI = GD.Load<PackedScene>("res://src/ui/shop_ui.tscn");

    [Node]
    public required MeshInstance3D Mesh { set; get; }

    [Node]
    public required Label3D ScoreLabel { set; get; }

    public bool inShop = false;

    public required Mesh InteractionMesh
    {
        get => Mesh.Mesh;
        set;
    }

    // TODO(j) pass these in from a resource to make unique shop stocks simple
    static readonly Dictionary<StringName, ShopItem> ShopItems = GDExt.LoadFromFolder<ShopItem>(
        shopItemPath
    );
    static readonly Dictionary<StringName, ShopItem> ShopUpgrades = GDExt.LoadFromFolder<ShopItem>(
        shopUpgradePath
    );

    List<ShopItem> Upgrades = [];
    List<ShopItem> Items = [];

    public override void _Ready()
    {
        //ShopItems.ForEach((item) => Log.PrintLn(item.ItemName));
    }

    public static List<ShopItem> GetAllItems() => [.. ShopItems.Values];

    public static List<ShopItem> GetAllUpgrades() => [.. ShopUpgrades.Values];

    public void OnInteraction()
    {
        if (inShop)
            return;
        inShop = true;
        Audio.PlaySfx(Sfx.UIOpen);
        var shop = ShopUI.New(GetAllItems(), GetAllUpgrades(), this);
        AddChild(shop);
    }
}
