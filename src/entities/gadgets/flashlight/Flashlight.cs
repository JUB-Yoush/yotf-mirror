using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
using DependencyAttribute = Chickensoft.AutoInject.DependencyAttribute;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Flashlight : Item, IDroppable, IGiveLight
{
    public override void _Notification(int what) => this.Notify(what);

    public static readonly PackedScene Packed = GD.Load<PackedScene>("uid://d34ehugbf1dk7");

    [Node]
    public required SpotLight3D SpotLight { set; get; }

    [Node]
    public required MeshInstance3D Mesh { set; get; }

    [Node]
    public required Area3D LightArea { set; get; }

    [Node]
    public required RayCast3D LightRayNode { set; get; }

    public new PackedScene PackedScene => Packed;

    public new Mesh DropMesh => Mesh.Mesh;

    public Light3D LightSource => SpotLight;

    public RayCast3D LightRay => LightRayNode;

    [Export]
    private float batteryUseRate = 10;

    [Export]
    private float LightEnergy;

    Inventory Inventory = null!;

    private PlayerStats PlayerStats = null!;
    private Camera3D Camera = null!;
    private Player player = null!;
    private bool isOn = false;

    private readonly List<IPhotographable> trackedSubjects = [];

    public override void _Ready()
    {
        player = this.SceneRoot().GetNode<Player>()!;
        Camera = GetParent().GetParent().GetNode<CameraManager>().GetNode<Camera3D>()!;
        Inventory = GetParent<Inventory>();
        PlayerStats = GetParent().GetParent().GetNode<PlayerStats>()!;

        LightArea.BodyEntered += OnReceivedObject;
        LightArea.BodyExited += OnRemovedObject;
    }

    public void OnReceivedObject(Node3D body)
    {
        if (body is IPhotographable p && !p.IsModifier)
        {
            trackedSubjects.Add(p);
            p.OnReceivedLight(this);
            GD.Print($"Flashlight added light to {body.Name}");
        }
    }

    public void OnRemovedObject(Node3D body)
    {
        if (body is IPhotographable p)
        {
            trackedSubjects.Remove(p);
            p.OnRemovedLight(this);
            GD.Print($"Flashlight removed light from {body.Name}");
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!CurrentItem)
            return;
        if (@event.IsActionPressed("take_photo") && !player.IsInMenu)
        {
            Audio.PlaySfx(Sfx.Click);
            isOn = !isOn;
            SpotLight.LightEnergy = isOn ? LightEnergy : 0;
            LightArea.Monitoring = isOn;
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

    void IGiveLight.OnReceivedObject(Node3D body)
    {
        OnReceivedObject(body);
    }

    void IGiveLight.OnRemovedObject(Node3D body)
    {
        OnRemovedObject(body);
    }
}
