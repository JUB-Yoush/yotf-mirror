using System;
using System.Dynamic;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class InteractionRay : RayCast3D
{
    public override void _Notification(int what) => this.Notify(what);

    private IInteractable? currentCollision = null!;

    [Node]
    public required Inventory Inventory { set; get; }

    public override void _Ready()
    {
        TargetPosition = new(0, 0, -2f);
        CollideWithAreas = true;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("pickup"))
        {
            currentCollision?.OnInteraction();
            currentCollision = null;
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
