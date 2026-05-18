using System;
using System.Runtime.CompilerServices;
using Godot;
using DependencyAttribute = Chickensoft.AutoInject.DependencyAttribute;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Flashlight : Item
{
    public override void _Notification(int what) => this.Notify(what);

    public static new readonly PackedScene Packed = GD.Load<PackedScene>("uid://d34ehugbf1dk7");

    [Node]
    public required SpotLight3D SpotLight { set; get; }

    [Node]
    public required MeshInstance3D Mesh { set; get; }

    [Export]
    private float batteryUseRate = 10;

    [Export]
    private float LightEnergy;

    Inventory Inventory = null!;

    private PlayerStats PlayerStats = null!;
    private Camera3D Camera = null!;

    public override void _Ready()
    {
        Camera = GetParent().GetParent().GetNode<CameraManager>().GetNode<Camera3D>()!;
        Inventory = GetParent<Inventory>();
        PlayerStats = GetParent().GetParent().GetNode<PlayerStats>()!;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!CurrentItem)
            return;

        if (Input.IsActionPressed("look_cam"))
        {
            PlayerStats.Battery -= (float)(batteryUseRate * delta);
        }

        if (Input.IsActionJustPressed("drop_item"))
        {
            var dropItem = MakeDropItem(Mesh.Mesh, Packed);

            dropItem.GlobalTransform = Camera.GlobalTransform;
            GetTree().CurrentScene.AddChild(dropItem);
            Inventory.RemoveCurrentItem();
        }

        Mesh.GlobalTransform = Camera.GlobalTransform;
        Mesh.GlobalPosition += (-Mesh.GlobalBasis.Z / 2) + (Mesh.GlobalBasis.X / 2);
        GlobalTransform = Camera.GlobalTransform;
    }
}
