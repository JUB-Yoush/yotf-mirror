using System;
using Godot;

public partial class Flashlight : Node3D, IItem
{
    SpotLight3D SpotLight = null!;

    Camera3D Camera = null!;
    bool on = false;

    public override void _Ready()
    {
        SpotLight = GetNode<SpotLight3D>("SpotLight3D");
        Camera = GetParent().GetParent().GetNode<Camera3D>("%Camera3D");
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
        SpotLight.GlobalTransform = Camera.GlobalTransform;
    }

    Texture2D IItem.GetIcon() => IItem.moonin;

    string IItem.GetItemName() => "Flashlight";
}
