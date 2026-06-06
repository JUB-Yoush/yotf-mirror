using System;
using Godot;

public partial class Graphmakertest : Node3D
{
    public override void _Ready()
    {
        var gdscript = GD.Load<GDScript>("res://addons/graphmaker/graphmaker.gd");
    }
}
