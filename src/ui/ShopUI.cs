using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class ShopUI : Control
{
    public override void _Notification(int what) => this.Notify(what);

    public static readonly PackedScene ShopItemView = GD.Load<PackedScene>("uid://c8frlegdjskm3");

    private List<ShopItem> Items = [];
    private List<ShopItem> Upgrades = [];

    [Node]
    public required HBoxContainer UpgradesView { set; get; }

    [Node]
    public required HBoxContainer ItemsView { set; get; }

    private ShopKiosk kiosk = null!;

    public static ShopUI New(List<ShopItem> items, List<ShopItem> upgrades, ShopKiosk kiosk)
    {
        var shop = ShopItemView.Instantiate<ShopUI>();
        shop.Items = items;
        shop.Upgrades = upgrades;
        shop.kiosk = kiosk;
        return shop;
    }

    public override void _Ready()
    {
        this.GetNode<Button>()!.Pressed += CloseShop;
        var player = this.SceneRoot().GetNode<PlayerController>()!;
        player.IsInMenu = true;
        Input.SetMouseMode(Input.MouseModeEnum.Visible);

        PopulateShop();
    }

    private void PopulateShop()
    {
        ItemsView.RemoveAllChildren();
        UpgradesView.RemoveAllChildren();

        var player = this.SceneRoot().GetNode<PlayerStats>(true)!;
        foreach (var item in Items)
        {
            var view = ShopItemView.Instantiate<VBoxContainer>();
            view.GetNode<TextureRect>("TextureRect").Texture = item.Icon;
            view.GetNode<Label>("Name").Text = item.ItemName;
            view.GetNode<Label>("Price").Text = $"${item.Price}";
            view.GetNode<Button>("Button").Pressed += () => BuyItem(item);
            view.GetNode<Button>("Button").Disabled = player.Money < item.Price;
            ItemsView.AddChild(view);
        }

        foreach (var upgrade in Upgrades)
        {
            var view = ShopItemView.Instantiate<VBoxContainer>();
            view.GetNode<TextureRect>("TextureRect").Texture = upgrade.Icon;
            view.GetNode<Label>("Name").Text = upgrade.ItemName;
            view.GetNode<Label>("Price").Text = $"${upgrade.Price}";
            view.GetNode<Button>("Button").Pressed += () => BuyUpgrade(upgrade);
            view.GetNode<Button>("Button").Disabled = player.Money < upgrade.Price;
            UpgradesView.AddChild(view);
        }
    }

    private void BuyUpgrade(ShopItem upgrade)
    {
        var player = this.SceneRoot().GetNode<PlayerStats>(true)!;
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
        var player = this.SceneRoot().GetNode<PlayerStats>(true)!;
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
