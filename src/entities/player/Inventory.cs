using System.Diagnostics;
using Godot;

namespace Yotf;

public partial class Inventory : Node3D
{
    const int Capacity = 4;
    private int currentIndex = 0;
    HBoxContainer Icons = null!;
    Item?[] InventoryArr
    {
        get
        {
            Item?[] res = new Item[Capacity];
            for (int i = 0; i < Capacity; i++)
                res[i] = GetChildOrNull<Item>(i);
            return res;
        }
    }

    public override void _Ready()
    {
        Icons = GetParent().GetNode<HBoxContainer>("HUD/InventoryIcons");
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
        InventoryArr[currentIndex]?.Exit();
        currentIndex = index;
        InventoryArr[currentIndex]?.Visible = true;
        InventoryArr[currentIndex]?.CurrentItem = true;
        GD.Print($"set {InventoryArr[currentIndex]?.ItemName} to current");
        GD.Print(
            $"{InventoryArr[currentIndex]?.ItemName} is now {InventoryArr[currentIndex]?.CurrentItem}"
        );
        Icons.GetChild<TextureRect>(currentIndex).Modulate = Color.Color8(255, 255, 255);
        ClearOtherItems(currentIndex);
        InventoryArr[currentIndex]?.Enter();
    }

    private void ClearOtherItems(int notThisOne)
    {
        for (int i = 0; i < Capacity; i++)
        {
            if (i == notThisOne)
                continue;
            InventoryArr[i]?.Visible = false;
            Icons.GetChild<TextureRect>(i).Modulate = Color.Color8(64, 46, 46);
            InventoryArr[i]?.CurrentItem = false;
        }
    }

    void AddItem(Item item)
    {
        Debug.Assert(InventoryArr[currentIndex] == null);
        item.Name = currentIndex.ToString();
        item.InInventory = true;
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
        Debug.Assert(currentIndex < Capacity);
        return InventoryArr[currentIndex]!;
    }

    // public override void _PhysicsProcess(double delta)
    // {
    //     InventoryArr[currentIndex]?.Update(delta);
    // }
}
