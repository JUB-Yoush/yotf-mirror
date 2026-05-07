using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

public partial class ShopUI : Control
{
    public static readonly PackedScene ShopItemView = GD.Load<PackedScene>("uid://c8frlegdjskm3");

    List<ShopItem> Items = [];
    List<ShopItem> Upgrades = [];
    HBoxContainer UpgradeContainer = null!;
    HBoxContainer ItemContainer = null!;

    public ShopUI Init(List<ShopItem> items, List<ShopItem> upgrades)
    {
        this.Items = items;
        this.Upgrades = upgrades;
        return this;
    }

    public override void _Ready()
    {
        UpgradeContainer = GetNode<HBoxContainer>("%Upgrades");
        ItemContainer = GetNode<HBoxContainer>("%Items");
        Items.Add(GD.Load<ShopItem>("uid://b23k3n6uvsqhm"));
        Upgrades.Add(GD.Load<ShopItem>("uid://dkxdiu2kqqy1k"));
        PopulateShop();
    }

    private void PopulateShop()
    {
        foreach (var item in Items)
        {
            var view = ShopItemView.Instantiate<VBoxContainer>();
            view.GetNode<TextureRect>("TextureRect").Texture = item.Icon;
            view.GetNode<Label>("Name").Text = item.ItemName;
            view.GetNode<Label>("Price").Text = $"${item.Price}";
            ItemContainer.AddChild(view);
        }

        foreach (var upgrade in Upgrades)
        {
            var view = ShopItemView.Instantiate<VBoxContainer>();
            view.GetNode<TextureRect>("TextureRect").Texture = upgrade.Icon;
            view.GetNode<Label>("Name").Text = upgrade.ItemName;
            view.GetNode<Label>("Price").Text = $"${upgrade.Price}";
            UpgradeContainer.AddChild(view);
        }
    }
}
