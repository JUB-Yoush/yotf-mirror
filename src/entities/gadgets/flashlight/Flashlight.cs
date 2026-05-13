using System;
using Godot;

namespace Yotf;

public partial class Flashlight : Item
{
    public static new readonly PackedScene Packed = GD.Load<PackedScene>("uid://d34ehugbf1dk7");

    private SpotLight3D spotLight = null!;
    private Camera3D camera = null!;
    private MeshInstance3D mesh = null!;
    private Inventory inventory = null!;
    private PlayerStats playerStats = null!;

    [Export]
    private float batteryUseRate = 10;

    [Export]
    private float LightEnergy;

    public override void _Ready()
    {
        ItemName = "Flashlight";
        spotLight = GetNode<SpotLight3D>("SpotLight3D");
        camera = GetParent().GetParent().GetNode<Camera3D>("%Camera3D");
        mesh = GetNode<MeshInstance3D>("MeshInstance3D");
        inventory = GetParent<Inventory>();
        playerStats = GetParent().GetParent().GetNode<PlayerStats>("Stats");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!CurrentItem)
            return;

        if (Input.IsActionPressed("look_cam"))
        {
            //spotLight.LightEnergy = LightEnergy;
            playerStats.Battery -= (float)(batteryUseRate * delta);
        }
        else
        {
            //spotLight.LightEnergy = 0;
        }

        if (Input.IsActionJustPressed("drop_item"))
        {
            var dropItem = MakeDropItem(mesh.Mesh, Packed);

            dropItem.GlobalTransform = camera.GlobalTransform;
            GetTree().CurrentScene.AddChild(dropItem);
            inventory.RemoveCurrentItem();
        }

        mesh.GlobalTransform = camera.GlobalTransform;
        mesh.GlobalPosition += (-mesh.GlobalBasis.Z / 2) + (mesh.GlobalBasis.X / 2);
        GlobalTransform = camera.GlobalTransform;
    }
}
