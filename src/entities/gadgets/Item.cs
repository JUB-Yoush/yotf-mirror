using System;
using Godot;

public abstract partial class Item : Node3D
{
    [Export]
    public string ItemName = "";

    [Export]
    public Texture2D Icon;
}
