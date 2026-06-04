using System;
using System.Diagnostics;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Inventory : Node3D
{
    public override void _Notification(int what) => this.Notify(what);

    public const int Capacity = 4;
    private int currentIndex = 0;

    [Node]
    public required Hud HUD { set; get; }

    public Item?[] Items
    {
        get
        {
            Item?[] res = new Item[Capacity];
            for (int i = 0; i < Capacity; i++)
                res[i] = GetNodeOrNull<Item>(i.ToString());
            return res;
        }
    }

    public override void _Ready()
    {
        AddItem(PhotoCamera.Packed.Instantiate<Item>(), 0);
        AddItem(Flashlight.Packed.Instantiate<Item>(), 1);
        AddItem(DisposableRestore.Packed.Instantiate<Item>(), 2);
        AddItem(FirecrackerItem.Packed.Instantiate<Item>(), 3);
        SetCurrentItem(0);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("set_item_1"))
        {
            SetCurrentItem(0);
        }
        else if (@event.IsActionPressed("set_item_2"))
        {
            SetCurrentItem(1);
        }
        else if (@event.IsActionPressed("set_item_3"))
        {
            SetCurrentItem(2);
        }
        else if (@event.IsActionPressed("set_item_4"))
        {
            SetCurrentItem(3);
        }
    }

    private void SetCurrentItem(int index)
    {
        HUD?.SelectSlot(index);
        Items[currentIndex]?.Unequipped();
        currentIndex = index;
        Items[currentIndex]?.CurrentItem = true;
        ClearItems(currentIndex);
        Items[currentIndex]?.Equipped();
    }

    private void ClearItems(int notThisOne = -1)
    {
        for (int i = 0; i < Capacity; i++)
        {
            if (i == notThisOne)
                continue;
            Items[i]?.CurrentItem = false;
        }
    }

    //TODO (j) consolidate these two functions.
    public void AddItem(Item item)
    {
        Debug.Assert(Items[currentIndex] == null);
        item.Name = currentIndex.ToString();
        item.InInventory = true;
        AddChild(item);
        SetCurrentItem(currentIndex);
        HUD.SetItemSlot(currentIndex, item.Icon);
        item.Added();
    }

    public void AddItem(Item item, int index, bool removeIfFilled = false)
    {
        if (Items[index] != null && !removeIfFilled)
            return;

        Items[index]?.QueueFree();

        item.Name = index.ToString();
        item.InInventory = true;
        AddChild(item);

        HUD.SetItemSlot(index, item.Icon);
        item.Added();
    }

    public void RemoveItem(int index)
    {
        if (Items[index] == null)
            return;

        var item = GetNode<Item>(index.ToString());
        HUD.SetItemSlot(index, null);
        item.Removed();
        item.QueueFree();
    }

    public void RemoveCurrentItem()
    {
        RemoveItem(currentIndex);
    }

    public Item GetEqippedItem()
    {
        Debug.Assert(currentIndex < Capacity);
        return Items[currentIndex]!;
    }

    public int GetItemIndex(string itemName)
    {
        for (int i = 0; i < Capacity; i++)
        {
            var item = GetNodeOrNull<Item>(i.ToString());
            if (item != null && item.ItemName == itemName)
                return i;
        }
        return -1;
    }
}
