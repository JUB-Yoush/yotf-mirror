using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

public partial class ShopUI : Control
{
    public static readonly PackedScene ShopItemView = GD.Load<PackedScene>("uid://c8frlegdjskm3");

    private List<ShopItem> Items = [];
    private List<ShopItem> Upgrades = [];
    private HBoxContainer UpgradeContainer = null!;
    private HBoxContainer ItemContainer = null!;
    private ShopKiosk kiosk = null!;

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
        ItemContainer.RemoveAllChildren();
        UpgradeContainer.RemoveAllChildren();

        var player = GetTree().CurrentScene.GetNode<PlayerStats>("Player/Stats");
        foreach (var item in Items)
        {
            var view = ShopItemView.Instantiate<VBoxContainer>();
            view.GetNode<TextureRect>("TextureRect").Texture = item.Icon;
            view.GetNode<Label>("Name").Text = item.ItemName;
            view.GetNode<Label>("Price").Text = $"${item.Price}";
            view.GetNode<Button>("Button").Pressed += () => BuyItem(item);
            view.GetNode<Button>("Button").Disabled = player.Money < item.Price;
            ItemContainer.AddChild(view);
        }

        foreach (var upgrade in Upgrades)
        {
            var view = ShopItemView.Instantiate<VBoxContainer>();
            view.GetNode<TextureRect>("TextureRect").Texture = upgrade.Icon;
            view.GetNode<Label>("Name").Text = upgrade.ItemName;
            view.GetNode<Label>("Price").Text = $"${upgrade.Price}";
            view.GetNode<Button>("Button").Pressed += () => BuyUpgrade(upgrade);
            view.GetNode<Button>("Button").Disabled = player.Money < upgrade.Price;
            UpgradeContainer.AddChild(view);
        }
    }

    private void BuyUpgrade(ShopItem upgrade)
    {
        var player = GetTree().CurrentScene.GetNode<PlayerStats>("Player/Stats");
        player.Money -= upgrade.Price;

        switch (upgrade.upgrade)
        {
            case ShopItem.Upgrade.Oxygen:
                player.MaxOxygen += 25;
                player.Oxygen = player.MaxOxygen;
                break;
            case ShopItem.Upgrade.Battery:
                player.MaxBattery += 25;
                player.Battery = player.MaxBattery;
                break;
        }

        PopulateShop();
    }

    private void BuyItem(ShopItem item)
    {
        var player = this.SceneRoot().GetNode<PlayerStats>()!;
        player.Money -= item.Price;
        var itemDrop = DroppedItem.New(item.itemScene.Instantiate<Item>().DropMesh, item.itemScene);
        itemDrop.GlobalTransform = kiosk.GlobalTransform;
        GetTree().CurrentScene.AddChild(itemDrop);
        PopulateShop();
    }

    private void CloseShop()
    {
        kiosk.inShop = false;
        var player = GetTree().CurrentScene.GetNode<PlayerController>("Player");
        player.IsInMenu = false;
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
        QueueFree();
    }
}
