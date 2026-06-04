using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class ShopKiosk : Node3D, IInteractable
{
    public override void _Notification(int what) => this.Notify(what);

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
    List<ShopItem> Items = [];
    List<ShopItem> Upgrades = [];

    public override void _Ready()
    {
        Items = [(GD.Load<ShopItem>("res://assets/data/shop_items/camera_item.tres"))];
        Upgrades = [(GD.Load<ShopItem>("res://assets/data/shop_items/oxygen_up.tres"))];
    }

    public void OnInteraction()
    {
        if (inShop)
            return;
        inShop = true;
        var shop = ShopUI.New(Items, Upgrades, this);
        AddChild(shop);
    }
}
