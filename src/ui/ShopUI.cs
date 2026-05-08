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
    ShopKiosk kiosk = null!;

    public ShopUI Init(List<ShopItem> items, List<ShopItem> upgrades, ShopKiosk kiosk)
    {
        this.Items = items;
        this.Upgrades = upgrades;
        this.kiosk = kiosk;
        return this;
    }

    public override void _Ready()
    {
        UpgradeContainer = GetNode<HBoxContainer>("%Upgrades");
        ItemContainer = GetNode<HBoxContainer>("%Items");
        GetNode<Button>("ReturnBtn").Pressed += CloseShop;

        var player = GetTree().CurrentScene.GetNode<PlayerController>("Player");
        player.IsInMenu = true;
        Input.SetMouseMode(Input.MouseModeEnum.Visible);

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
            view.GetNode<Button>("Button").Pressed += () => BuyItem(item);
            ItemContainer.AddChild(view);
        }

        foreach (var upgrade in Upgrades)
        {
            var view = ShopItemView.Instantiate<VBoxContainer>();
            view.GetNode<TextureRect>("TextureRect").Texture = upgrade.Icon;
            view.GetNode<Label>("Name").Text = upgrade.ItemName;
            view.GetNode<Label>("Price").Text = $"${upgrade.Price}";
            view.GetNode<Button>("Button").Pressed += () => BuyUpgrade(upgrade);
            UpgradeContainer.AddChild(view);
        }
    }

    private void BuyUpgrade(ShopItem upgrade) { }

    private void BuyItem(ShopItem item)
    {
        var itemDrop = DroppedItem
            .Packed.Instantiate<DroppedItem>()
            .Init(item.itemScene.Instantiate<Item>().DropMesh, item.itemScene);
        itemDrop.GlobalTransform = kiosk.GlobalTransform;
        GetTree().CurrentScene.AddChild(itemDrop);
    }

    private void CloseShop()
    {
        kiosk.inShop = false;
        var player = GetTree().CurrentScene.GetNode<PlayerController>("Player");
        player.IsInMenu = true;
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
        QueueFree();
    }
}
