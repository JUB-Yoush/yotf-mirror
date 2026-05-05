using System;
using Godot;

namespace Yotf;

public partial class Item : Node3D
{
    public enum State
    {
        InInventory,
        OnGround,
    }

    public static readonly Texture2D DefaultTexture = GD.Load<Texture2D>(
        "res://assets/2d/mooninicon.png"
    );

    public State ItemState = State.OnGround;
    public string ItemName = "default_item_name";
    public Texture2D Icon = DefaultTexture;
    public bool InInventory = false;
    public bool CurrentItem = false;

    public virtual void Enter() { }

    public virtual void Exit() { }
}
