using System.Diagnostics;
using Godot;

namespace Yotf;

public partial class Inventory : Node3D
{
    const int Capacity = 4;
    private int currentIndex = 0;
    private HBoxContainer Icons = null!;
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
        Icons = GetParent().GetNode<HBoxContainer>("HUD/InventoryIcons");
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
        InventoryArr[currentIndex]?.Exit();
        currentIndex = index;
        InventoryArr[currentIndex]?.Visible = true;
        InventoryArr[currentIndex]?.CurrentItem = true;
        Icons.GetChild<TextureRect>(currentIndex).Modulate = Color.Color8(255, 255, 255);
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
            Icons.GetChild<TextureRect>(i).Modulate = Color.Color8(64, 46, 46);
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
        Icons.GetChild<TextureRect>(currentIndex).Texture = item.Icon;
    }

    public void AddItem(Item item, int index, bool removeIfFilled = false)
    {
        if (InventoryArr[index] != null && !removeIfFilled)
            return;

        InventoryArr[index]?.QueueFree();

        item.Name = index.ToString();
        item.InInventory = true;
        AddChild(item);
        Icons.GetChild<TextureRect>(index).Texture = item.Icon;
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
}
