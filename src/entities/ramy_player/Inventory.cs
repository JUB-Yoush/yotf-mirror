using System.Diagnostics;
using Godot;

public partial class Inventory : Node3D
{
    int capacity = 4;
    int currentIndex = 0;
    HBoxContainer Icons = null!;
    IItem?[] InventoryArr
    {
        get
        {
            IItem?[] res = new IItem[capacity];
            for (int i = 0; i < capacity; i++)
                res[i] = GetChildOrNull<IItem>(i);
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
        currentIndex = index;
        InventoryArr[currentIndex]?.GetNode()?.Visible = true;
        Icons.GetChild<TextureRect>(currentIndex).Modulate = Color.Color8(255, 255, 255);

        for (int i = 0; i < capacity; i++)
        {
            if (i == currentIndex)
                continue;
            GetChildOrNull<IItem>(i)?.GetNode().Visible = false;
            Icons.GetChild<TextureRect>(i).Modulate = Color.Color8(64, 46, 46);
        }
    }

    void AddItem(IItem item)
    {
        Debug.Assert(InventoryArr[currentIndex] == null);
        item.GetNode().Name = currentIndex.ToString();
        AddChild(item.GetNode());
    }

    void RemoveItem(int index)
    {
        if (InventoryArr[index] != null)
            return;
        GetChild<IItem>(index).GetNode().QueueFree();
    }

    public IItem GetEqippedItem()
    {
        Debug.Assert(currentIndex < capacity);
        return InventoryArr[currentIndex]!;
    }

    public override void _PhysicsProcess(double delta)
    {
        InventoryArr[currentIndex]?.Update(delta);
    }
}
