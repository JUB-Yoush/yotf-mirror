using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class FirecrackerItem : Item, IDroppable
{
    public override void _Notification(int what) => this.Notify(what);

    public static readonly PackedScene Packed = GD.Load<PackedScene>(
        "res://src/entities/gadgets/firecracker/firecracker_item.tscn"
    );

    public new PackedScene PackedScene => Packed;

    public new Mesh DropMesh => Mesh.Mesh;

    [Node]
    public required MeshInstance3D Mesh { get; set; }

    [Export]
    float throwForce = 4f;

    public Player player = null!;
    public Camera3D playerCamera = null!;

    public override void _Ready()
    {
        player = GetParent().GetParent<Player>();
        playerCamera = player.GetNode<CameraManager>().GetNode<Camera3D>()!;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!CurrentItem)
            return;

        if (@event.IsActionPressed("take_photo"))
        {
            MakeFirecracker();
            stock--;
            if (stock == 0)
            {
                player.Inventory.RemoveCurrentItem();
            }
            player.HUD.InventoryText[Index].Text = $"[center][b]{Index + 1} x{stock}";
        }

        if (@event.IsActionPressed("drop_item"))
        {
            var dropItem = IDroppable.MakeDropItem(this);
            dropItem.stock = stock;
            dropItem.GlobalTransform = playerCamera.GlobalTransform;
            GetTree().CurrentScene.AddChild(dropItem);
            dropItem.GlobalTransform = playerCamera.GlobalTransform;
            dropItem.GlobalPosition += -playerCamera.GlobalTransform.Basis.Z;
            player.Inventory.RemoveCurrentItem();
        }
    }

    public void MakeFirecracker()
    {
        var initialVelocity = -playerCamera.GlobalTransform.Basis.Z * throwForce;
        var firecracker = Firecracker.New(this.SceneRoot(), initialVelocity);
        //this.SceneRoot().AddChild(firecracker);
        firecracker.GlobalPosition = GlobalPosition + -playerCamera.GlobalTransform.Basis.Z;
    }

    public override void _Process(double delta)
    {
        GlobalTransform = playerCamera.GlobalTransform;
        if (!CurrentItem)
            return;

        Mesh.GlobalPosition += (-Mesh.GlobalBasis.Z / 2) + (Mesh.GlobalBasis.X / 2); //+ new Vec3(0, 0, 2);
    }
}
