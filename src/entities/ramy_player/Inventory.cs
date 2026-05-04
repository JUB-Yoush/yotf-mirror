using System;
using System.Diagnostics;
using Godot;

public partial class Inventory : Node
{
    int capacity = 4;
    int currentIndex = 0;
    Item?[] InventoryArr
    {
        get
        {
            Item?[] res = new Item[capacity];
            for (int i = 0; i < capacity; i++)
                res[i] = GetChildOrNull<Item>(i);
            return res;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("set_item_1"))
        {
            SetCurrentItem(1);
        }
        else if (@event.IsActionPressed("set_item_2"))
        {
            SetCurrentItem(2);
        }
        else if (@event.IsActionPressed("set_item_3"))
        {
            SetCurrentItem(3);
        }
        else if (@event.IsActionPressed("set_item_4"))
        {
            SetCurrentItem(4);
        }
    }

    // Item?[] GetInventory()
    // {
    //     Item?[] res = new Item[capacity];
    //     for (int i = 0; i < capacity; i++)
    //         res[i] = GetChildOrNull<Item>(0);
    //     return res;
    // }

    private void SetCurrentItem(int index)
    {
        if (InventoryArr[index] == null)
            return;

        currentIndex = index;
    }

    void AddItem(Item item)
    {
        Debug.Assert(InventoryArr[currentIndex] == null);
        item.Name = currentIndex.ToString();
        AddChild(item);
    }

    void RemoveItem(int index)
    {
        if (InventoryArr[index] != null)
            return;
        GetChild<Item>(index).QueueFree();
    }

    public Item GetEqippedItem()
    {
        Debug.Assert(currentIndex < capacity);
        return InventoryArr[currentIndex]!;
    }
}
