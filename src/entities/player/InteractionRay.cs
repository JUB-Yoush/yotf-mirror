using System;
using System.Dynamic;
using Godot;

namespace Yotf;

public partial class InteractionRay : RayCast3D
{
    DroppedItem? currentCollision = null!;
    Inventory inventory = null!;

    public override void _Ready()
    {
        inventory = GetNode<Inventory>("%Inventory");

        TargetPosition = new(0, 0, -2f);
        CollideWithAreas = true;
    }

    public override void _Input(InputEvent @event)
    {
        if (
            @event.IsActionPressed("pickup")
            && currentCollision != null
            && inventory.GetEqippedItem() == null
        )
        {
            inventory.AddItem(currentCollision.ItemRef.Instantiate<Item>());
            currentCollision.QueueFree();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsColliding())
        {
            currentCollision = null;
            return;
        }
        currentCollision = ((Area3D)GetCollider()).GetParent<DroppedItem>();
    }
}
