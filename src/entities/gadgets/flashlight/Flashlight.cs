using System;
using System.Runtime.CompilerServices;
using Godot;
using DependencyAttribute = Chickensoft.AutoInject.DependencyAttribute;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Flashlight : Item, IDroppable
{
    public override void _Notification(int what) => this.Notify(what);

    public static readonly PackedScene Packed = GD.Load<PackedScene>("uid://d34ehugbf1dk7");

    [Node]
    public required SpotLight3D SpotLight { set; get; }

    [Node]
    public required MeshInstance3D Mesh { set; get; }

    public new PackedScene PackedScene => Packed;

    public new Mesh DropMesh => Mesh.Mesh;

    [Export]
    private float batteryUseRate = 10;

    [Export]
    private float LightEnergy;

    Inventory Inventory = null!;

    private PlayerStats PlayerStats = null!;
    private Camera3D Camera = null!;
    private bool isOn = false;

    public override void _Ready()
    {
        Camera = GetParent().GetParent().GetNode<CameraManager>().GetNode<Camera3D>()!;
        Inventory = GetParent<Inventory>();
        PlayerStats = GetParent().GetParent().GetNode<PlayerStats>()!;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("toggle_flashlight"))
        {
            isOn = !isOn;
            SpotLight.LightEnergy = isOn ? 10 : 0;
        }
    }

    public override void _Process(double delta)
    {
        Mesh.GlobalTransform = Camera.GlobalTransform;
        Mesh.GlobalPosition += (-Mesh.GlobalBasis.Z / 2) + (Mesh.GlobalBasis.X / 2);
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalTransform = Camera.GlobalTransform;

        if (isOn)
        {
            PlayerStats.Battery -= (float)(batteryUseRate * delta);
        }

        if (!CurrentItem)
            return;

        if (Input.IsActionJustPressed("look_cam"))
        {
            PlayerStats.Battery -= (float)(batteryUseRate * delta);
        }

        if (Input.IsActionJustPressed("drop_item"))
        {
            var dropItem = IDroppable.MakeDropItem(this);
            dropItem.GlobalTransform = Camera.GlobalTransform;
            GetTree().CurrentScene.AddChild(dropItem);
            Inventory.RemoveCurrentItem();
        }
    }

    public override void Equipped()
    {
        Mesh.Visible = true;
    }

    public override void Unequipped()
    {
        Mesh.Visible = false;
    }
}
