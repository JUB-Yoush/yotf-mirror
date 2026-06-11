using System;
using System.Text;
using Godot;

namespace Yotf;

public partial class Item : Node3D, IDroppable
{
    public static readonly Texture2D DefaultTexture = GD.Load<Texture2D>(
        "res://assets/2d/mooninicon.png"
    );

    [Export]
    public string ItemName = "default_item_name";

    [Export]
    public Texture2D Icon = DefaultTexture;

    [Export]
    float restoreAmount = 50;

    [Export]
    public bool hasCount = false;

    [Export]
    public int stock = 5;

    [Export]
    public string[] Instructions = [];

    public bool InInventory = false;
    public bool CurrentItem = false;
    public int currentIndex = -1;

    public PackedScene PackedScene => throw new NotImplementedException();

    public Mesh DropMesh => throw new NotImplementedException();

    public virtual void Added() { }

    public virtual void Removed() { }

    public virtual void Equipped()
    {
        Visible = true;
    }

    public virtual void Unequipped()
    {
        Visible = false;
    }

    public string GetInstructions()
    {
        StringBuilder result = new();
        Array.ForEach<string>(
            Instructions,
            (instruction) => result.Append(instruction).Append('\n')
        );
        return result.ToString();
    }

    public override void _Input(InputEvent @event) { }
}
