using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class ShopUI : Control
{
    public override void _Notification(int what) => this.Notify(what);

    public static readonly PackedScene Packed = GD.Load<PackedScene>("res://src/ui/shop_ui.tscn");
    public static readonly PackedScene ShopItemView = GD.Load<PackedScene>(
        "res://src/ui/shop_item.tscn"
    );

    private List<ShopItem> Items = [];
    private List<ShopItem> Upgrades = [];

    [Node]
    public required HBoxContainer UpgradeView { set; get; }

    [Node]
    public required HBoxContainer ItemView { set; get; }

    private ShopKiosk kiosk = null!;

    public static ShopUI New(List<ShopItem> items, List<ShopItem> upgrades, ShopKiosk kiosk)
    {
        var shop = Packed.Instantiate<ShopUI>();
        shop.Items = items;
        shop.Upgrades = upgrades;
        shop.kiosk = kiosk;
        return shop;
    }

    public override void _Ready()
    {
        this.GetNode<Button>()!.Pressed += CloseShop;
        var player = this.SceneRoot().GetNode<Player>()!;
        player.IsInMenu = true;
        Input.SetMouseMode(Input.MouseModeEnum.Visible);

        PopulateShop();
    }

    private void PopulateShop()
    {
        ItemView.RemoveAllChildren();
        UpgradeView.RemoveAllChildren();

        var player = this.SceneRoot().GetNode<Player>()!.GetNode<PlayerStats>(true)!;
        foreach (var item in Items)
        {
            var view = ShopItemView.Instantiate<VBoxContainer>();
            view.GetNode<TextureRect>("TextureRect").Texture = item.Icon;
            view.GetNode<Label>("Name").Text = item.Name;
            view.GetNode<Label>("Price").Text = $"${item.Price}";
            view.GetNode<Button>("Button").Pressed += () => BuyItem(item);
            view.GetNode<Button>("Button").Disabled = player.Money < item.Price;
            ItemView.AddChild(view);
        }

        foreach (var upgrade in Upgrades)
        {
            var view = ShopItemView.Instantiate<VBoxContainer>();
            view.GetNode<TextureRect>("TextureRect").Texture = upgrade.Icon;
            view.GetNode<Label>("Name").Text = upgrade.Name;
            view.GetNode<Label>("Price").Text = $"${upgrade.Price}";
            view.GetNode<Button>("Button").Pressed += () => BuyUpgrade(upgrade);
            view.GetNode<Button>("Button").Disabled = player.Money < upgrade.Price;
            UpgradeView.AddChild(view);
        }
    }

    private void BuyUpgrade(ShopItem upgrade)
    {
        var player = this.SceneRoot().GetNode<Player>()!.Stats;
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
            case ShopItem.Upgrade.Film:
                PlayerStats.MaxFilm += 3;
                break;
            case ShopItem.Upgrade.CameraZoom:
                PlayerStats.MaxZoom += 10;
                break;
            case ShopItem.Upgrade.SwimSpeed:
                PlayerStats.ExtraSwimSpeed += 1;
                break;
        }

        PopulateShop();
    }

    private void BuyItem(ShopItem item)
    {
        var player = this.SceneRoot().GetNode<Player>()!.Stats;
        player.Money -= item.Price;
        var mesh = item.DropMesh;
        var itemDrop = DroppedItem.New(mesh, item.itemScene);
        itemDrop.restore = item.restore;
        Log.PrintLn(itemDrop.restore, item.restore);
        GetTree().CurrentScene.AddChild(itemDrop);
        itemDrop.GlobalTransform = kiosk.GlobalTransform;
        itemDrop.GlobalPosition -= -kiosk.GlobalTransform.Basis.Z;

        if (item.restore != Disposable.Restore.None) { }
        PopulateShop();
    }

    private void CloseShop()
    {
        kiosk.inShop = false;
        var player = GetTree().CurrentScene.GetNode<Player>("Player");
        player.IsInMenu = false;
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
        QueueFree();
    }
}
