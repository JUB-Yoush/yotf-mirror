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

    bool firstTime = true;

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
            currentCollision?.RemoveOutlineMesh();
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
        if (firstTime)
        {
            firstTime = false;
            Inventory.GetParent<Player>().MakeAlert("PRESS F TO INTERACT");
        }
        currentCollision = ((Node)GetCollider()).GetParent<IInteractable>();
        currentCollision.OutlineMesh();
    }
}
