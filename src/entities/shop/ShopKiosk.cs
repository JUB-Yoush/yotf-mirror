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

    // TODO(j) pass these in from a resource to make unique shop stocks simple
    List<ShopItem> Items = [];
    List<ShopItem> Upgrades = [];

    public override void _Ready()
    {
        Items = [(GD.Load<ShopItem>("uid://b23k3n6uvsqhm"))];
        Upgrades = [(GD.Load<ShopItem>("uid://dkxdiu2kqqy1k"))];
    }

    public Mesh GetMesh() => Mesh.Mesh;

    public void OnInteraction()
    {
        if (inShop)
            return;
        inShop = true;
        var shop = ShopUI.New(Items, Upgrades, this);
        AddChild(shop);
    }
}
