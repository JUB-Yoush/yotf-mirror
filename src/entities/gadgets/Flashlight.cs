using System;
using Godot;

public partial class Flashlight : Node3D, IItem
{
    public void Update(double delta)
    {
        throw new NotImplementedException();
    }

    Texture2D IItem.GetIcon()
    {
        throw new NotImplementedException();
    }

    string IItem.GetItemName()
    {
        throw new NotImplementedException();
    }
}
