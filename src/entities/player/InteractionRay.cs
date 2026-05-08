using System;
using System.Dynamic;
using Godot;

namespace Yotf;

public partial class InteractionRay : RayCast3D
{
    private IInteractable? currentCollision = null!;
    private Inventory inventory = null!;

    public override void _Ready()
    {
        inventory = GetNode<Inventory>("%Inventory");

        TargetPosition = new(0, 0, -2f);
        CollideWithAreas = true;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("pickup") && currentCollision != null)
        {
            currentCollision.OnInteraction();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsColliding())
        {
            currentCollision?.RemoveOutlineMesh();
            currentCollision = null;
            return;
        }
        currentCollision = ((Node)GetCollider()).GetParent<IInteractable>();
        currentCollision.OutlineMesh();
    }
}
