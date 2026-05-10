using System;
using System.Diagnostics;
using Godot;

namespace Yotf;

public partial class Inventory : Node3D
{
    const int Capacity = 4;
    private int currentIndex = 0;

    public Hud? playerHUD;

    //TOOD (j) set up setter that adds node to scene tree, is there a way to get the value being passed into the
    private Item?[] InventoryArr
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
        playerHUD = GetNode<Hud>("%HUD");
        AddItem(PhotoCamera.Packed.Instantiate<Item>(), 0);
        AddItem(Flashlight.Packed.Instantiate<Item>(), 1);
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
        playerHUD?.SelectSlot(index);
        InventoryArr[currentIndex]?.Exit();
        currentIndex = index;
        InventoryArr[currentIndex]?.Visible = true;
        InventoryArr[currentIndex]?.CurrentItem = true;

        ClearItems(currentIndex);
        InventoryArr[currentIndex]?.Enter();
    }

    private void ClearItems(int notThisOne = -1)
    {
        for (int i = 0; i < Capacity; i++)
        {
            if (i == notThisOne)
                continue;
            InventoryArr[i]?.Visible = false;
            playerHUD?.ClearSlots();
            InventoryArr[i]?.CurrentItem = false;
        }
    }

    public void AddItem(Item item)
    {
        Debug.Assert(InventoryArr[currentIndex] == null);
        item.Name = currentIndex.ToString();
        item.InInventory = true;
        AddChild(item);
        SetCurrentItem(currentIndex);
        playerHUD?.SetItemSlot(currentIndex, item.Icon);
    }

    public void AddItem(Item item, int index, bool removeIfFilled = false)
    {
        if (InventoryArr[index] != null && !removeIfFilled)
            return;

        InventoryArr[index]?.QueueFree();

        item.Name = index.ToString();
        item.InInventory = true;
        AddChild(item);

        playerHUD?.SetItemSlot(index, item.Icon);
    }

    public void RemoveItem(int index)
    {
        if (InventoryArr[index] == null)
            return;
        GetChild<Item>(index).QueueFree();
    }

    public void RemoveCurrentItem()
    {
        RemoveItem(currentIndex);
    }

    public Item GetEqippedItem()
    {
        Debug.Assert(currentIndex < Capacity);
        return InventoryArr[currentIndex]!;
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
