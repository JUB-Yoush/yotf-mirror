using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

public partial class ShopKiosk : Node3D, IInteractable
{
    private static readonly PackedScene shopUI = GD.Load<PackedScene>("uid://pqjmq4icdsyr");
    Mesh mesh = null!;
    public bool inShop = false;

    // TODO(j) pass these in from a resource to make unique shop stocks simple
    List<ShopItem> Items = [];
    List<ShopItem> Upgrades = [];

    public override void _Ready()
    {
        mesh = GetNode<MeshInstance3D>("MeshInstance3D").Mesh;
        Items = [(GD.Load<ShopItem>("uid://b23k3n6uvsqhm"))];
        Upgrades = [(GD.Load<ShopItem>("uid://dkxdiu2kqqy1k"))];
    }

    public Mesh GetMesh() => mesh;

    public void OnInteraction()
    {
        if (inShop)
            return;
        inShop = true;
        var shop = shopUI.Instantiate<ShopUI>().Init(Items, Upgrades, this);
        AddChild(shop);
    }
}
