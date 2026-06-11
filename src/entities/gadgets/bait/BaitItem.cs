using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class BaitItem : Item, IDroppable
{
    public override void _Notification(int what) => this.Notify(what);

    public static readonly PackedScene Packed = GD.Load<PackedScene>(
        "res://src/entities/gadgets/bait/bait_item.tscn"
    );

    public new PackedScene PackedScene => Packed;

    public new Mesh DropMesh => Mesh.Mesh;

    [Node]
    public required MeshInstance3D Mesh { get; set; }

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
            MakeBait();
            stock--;
            if (stock == 0)
            {
                player.Inventory.RemoveCurrentItem();
            }

            player.HUD.InventoryText[Index].Text = $"[center][b]{Index + 1} x{base.stock}";
        }
    }

    private void MakeBait()
    {
        var bait = Bait.Packed.Instantiate<Bait>();
        this.SceneRoot().AddChild(bait);
        bait.GlobalPosition = GlobalPosition + -playerCamera.GlobalTransform.Basis.Z;
    }
}
