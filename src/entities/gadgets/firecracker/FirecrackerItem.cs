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

    [Export]
    int ammo = 5;

    public Player player = null!;
    public Camera3D playerCamera = null!;

    public override void _Ready()
    {
        player = GetParent().GetParent<Player>();
        playerCamera = player.GetNode<CameraManager>().GetNode<Camera3D>()!;
    }

    public override void _Input(InputEvent @event)
    {
        if (!CurrentItem)
            return;

        if (@event.IsActionPressed("take_photo"))
        {
            MakeFirecracker();
            ammo--;
            Log.PrintLn(ammo);
            if (ammo == 0)
            {
                player.Inventory.RemoveCurrentItem();
            }
        }
    }

    public void MakeFirecracker()
    {
        var initialVelocity = -playerCamera.GlobalTransform.Basis.Z * throwForce;
        var firecracker = Firecracker.New(initialVelocity);
        this.SceneRoot().AddChild(firecracker);
        firecracker.GlobalPosition =
            GlobalPosition + -playerCamera.GlobalTransform.Basis.Z * throwForce;
    }

    public override void _Process(double delta)
    {
        GlobalTransform = playerCamera.GlobalTransform;
        if (!CurrentItem)
            return;

        //TODO(j) where does this mesh go bruh
        Mesh.Scale = new(100, 100, 100);
        Mesh.GlobalPosition += (-Mesh.GlobalBasis.Z / 2) + (Mesh.GlobalBasis.X / 2); //+ new Vec3(0, 0, 2);
    }
}
