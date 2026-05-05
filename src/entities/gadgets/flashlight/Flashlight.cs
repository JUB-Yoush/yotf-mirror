using System;
using Godot;

namespace Yotf;

public partial class Flashlight : Item
{
    private SpotLight3D spotLight = null!;
    private Camera3D camera = null!;
    private MeshInstance3D mesh = null!;

    public override void _Ready()
    {
        ItemName = "Flashlight";
        Icon = Item.DefaultTexture;
        spotLight = GetNode<SpotLight3D>("SpotLight3D");
        camera = GetParent().GetParent().GetNode<Camera3D>("%Camera3D");
        mesh = GetNode<MeshInstance3D>("MeshInstance3D");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!CurrentItem)
            return;

        if (Input.IsActionPressed("look_cam"))
        {
            spotLight.LightEnergy = 10;
        }
        else
        {
            spotLight.LightEnergy = 0;
        }

        mesh.GlobalTransform = camera.GlobalTransform;
        mesh.GlobalPosition += (-mesh.GlobalBasis.Z / 2) + (mesh.GlobalBasis.X / 2);
        GlobalTransform = camera.GlobalTransform;
    }
}
