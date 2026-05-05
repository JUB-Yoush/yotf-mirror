using System;
using Godot;

public partial class Flashlight : Item
{
    SpotLight3D SpotLight = null!;

    Camera3D Camera = null!;
    MeshInstance3D mesh = null!;
    bool on = false;

    public override void _Ready()
    {
        ItemName = "Flashlight";
        Icon = Item.moonin;
        SpotLight = GetNode<SpotLight3D>("SpotLight3D");
        Camera = GetParent().GetParent().GetNode<Camera3D>("%Camera3D");
        mesh = GetNode<MeshInstance3D>("MeshInstance3D");
    }

    public override void Enter()
    {
        SpotLight.LightEnergy = 10;
    }

    public override void Exit()
    {
        on = false;
        SpotLight.LightEnergy = 0;
    }

    public override void _Input(InputEvent @event) { }

    public override void _PhysicsProcess(double delta)
    {
        if (!currentItem)
            return;

        if (Input.IsActionPressed("look_cam"))
        {
            SpotLight.LightEnergy = 10;
        }
        else
        {
            SpotLight.LightEnergy = 0;
        }

        mesh.GlobalTransform = Camera.GlobalTransform;
        mesh.GlobalPosition += (-mesh.GlobalBasis.Z / 2) + (mesh.GlobalBasis.X / 2);
        GlobalTransform = Camera.GlobalTransform;
    }
}
