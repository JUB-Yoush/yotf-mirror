using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class ShopUI : Control
{

    [Export]
    public required ButtonGroup ShopSortBtnGroup { set; get; }

    public override void _Notification(int what) => this.Notify(what);

    public static readonly PackedScene Packed = GD.Load<PackedScene>("res://src/ui/shop_ui.tscn");
    public static readonly PackedScene ShopItemView = GD.Load<PackedScene>(
        "res://src/ui/shop_item.tscn"
    );

    public static readonly PackedScene ShopItemQueue = GD.Load<PackedScene>("res://src/ui/queued_item.tscn");

    private List<ShopItem> Items = [];
    private List<ShopItem> Upgrades = [];

    private List<ShopItem> ItemQueue = []; //to purchase items
    private List<ShopItem> UpgradeQueue = [];

    // [Node]
    // public required HBoxContainer UpgradeView { set; get; }

    // [Node]
    // public required HBoxContainer ItemView { set; get; }

    [Node("%ShopItems/CollectionItems")]
    public required GridContainer CollectionItems { set; get; }

    [Node("%ShopItems/CollectionUpgrades")]
    public required GridContainer CollectionUpgrades { set; get; }

    [Node("PanelContainer/MarginContainer/ShopBody/CheckoutAndBuy/BuyButton")]
    public required TextureButton BuyButton { set; get; }

    [Node("%CartItemsList")]
    public required VBoxContainer CartItemList { set; get; }

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

        ShopSortBtnGroup?.Pressed += OnSortGroupPressed;
        BuyButton.Pressed += BuyAll;
        PopulateShop();

        
    
    }

    private void OnSortGroupPressed(BaseButton button)
    {
    

        if (button.Name == "SortBtnItems")
        {
           
            CollectionItems.Visible = true;
            CollectionUpgrades.Visible = false;
        }
        else if (button.Name == "SortBtnUpgrades")
        {
         
            CollectionItems.Visible = false;
            CollectionUpgrades.Visible = true;
         }
    }
    private void PopulateShop()
    {
       
        CollectionItems.RemoveAllChildren();
        CollectionUpgrades.RemoveAllChildren();

        var player = this.SceneRoot().GetNode<Player>()!.GetNode<PlayerStats>(true)!;
        foreach (var item in Items)
        {
            var view = ShopItemView.Instantiate<TextureButton>();
            view.GetNode<TextureRect>("TextureRect").Texture = item.Icon;
          view.GetNode<Label>("MarginContainer/VBoxContainer/ItemName").Text = item.Name;
            view.GetNode<Label>("MarginContainer/VBoxContainer/ItemPrice").Text = $"${item.Price}";
            view.GetNode<Label>("MarginContainer/VBoxContainer/ItemDesc").Text = item.Description;
            view.GetNode<TextureButton>(".").Pressed += () => QueueItem(item);
            view.GetNode<TextureButton>(".").Disabled = player.Money < item.Price;
            CollectionItems.AddChild(view);
        }

        foreach (var upgrade in Upgrades)
        {
            var view = ShopItemView.Instantiate<TextureButton>();
            view.GetNode<TextureRect>("TextureRect").Texture = upgrade.Icon;
            view.GetNode<Label>("MarginContainer/VBoxContainer/ItemName").Text = upgrade.Name;
            view.GetNode<Label>("MarginContainer/VBoxContainer/ItemPrice").Text = $"${upgrade.Price}";
            view.GetNode<Label>("MarginContainer/VBoxContainer/ItemDesc").Text = upgrade.Description;
            view.GetNode<TextureButton>(".").Pressed += () => QueueUpgrade(upgrade);
            view.GetNode<TextureButton>(".").Disabled = player.Money < upgrade.Price;
            CollectionUpgrades.AddChild(view);
        }
    }

    private void QueueItem(ShopItem item) {
    
        ItemQueue.Add(item);

        var queue = ShopItemQueue.Instantiate<Button>();
        queue.GetNode<Button>(".").Icon = item.Icon;
        queue.GetNode<Button>(".").Text = item.Name;
        CartItemList.AddChild(queue);
    }

    private void QueueUpgrade(ShopItem upgrade) {
   
        UpgradeQueue.Add(upgrade);

        var queue = ShopItemQueue.Instantiate<Button>();
        queue.GetNode<Button>(".").Icon = upgrade.Icon;
        queue.GetNode<Button>(".").Text = upgrade.Name;
        CartItemList.AddChild(queue);
    }

 

    private void BuyAll() {
        foreach (ShopItem item in ItemQueue) {
            BuyItem(item);
        }
        foreach (ShopItem item in UpgradeQueue) {
            BuyUpgrade(item);
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
