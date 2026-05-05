using System;
using Godot;

public partial class Flashlight : Node3D, IItem
{
    SpotLight3D SpotLight = null!;

    Camera3D Camera = null!;
    MeshInstance3D mesh = null!;
    bool on = false;
    bool inInventory = false;

    public override void _Ready()
    {
        SpotLight = GetNode<SpotLight3D>("SpotLight3D");
        Camera = GetParent().GetParent().GetNode<Camera3D>("%Camera3D");
        mesh = GetNode<MeshInstance3D>("MeshInstance3D");
    }

    public void Enter()
    {
        on = true;
        SpotLight.LightEnergy = 10;
    }

    public void Exit()
    {
        on = false;
        SpotLight.LightEnergy = 0;
    }

    public void Update(double delta)
    {
        if (!on)
            return;
        GlobalTransform = Camera.GlobalTransform;
        mesh.GlobalTransform = Camera.GlobalTransform;
        mesh.GlobalPosition += (-mesh.GlobalBasis.Z / 2) + (mesh.GlobalBasis.X / 2);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (inInventory)
            return;
    }

    Texture2D IItem.GetIcon() => IItem.moonin;

    string IItem.GetItemName() => "Flashlight";
}
