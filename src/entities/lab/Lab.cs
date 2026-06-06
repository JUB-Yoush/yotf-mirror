using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Lab : Node3D
{
    public override void _Notification(int what) => this.Notify(what);

    public static Action<Lab>? CurrentLabUpdated;

    static readonly HashSet<int> labIndicies = [];

    public static Lab? CurrentLab
    {
        set
        {
            field?.Toggle(false);
            field = value;
            field!.Toggle(true);
        }
        get;
    }

    [Node]
    public required StaticBody3D TopGate { set; get; }

    [Node]
    public required StaticBody3D BottomGate { set; get; }

    [Export]
    public int Index = 0;

    [Export]
    public float OxygenScale = 1f;

    [Export]
    public int requiredGalleryScore;

    [Export]
    public bool FinalLab;

    [Node]
    public required PhotoTerminal PhotoTerminal { set; get; }

    [Node]
    public required ShopKiosk ShopKiosk { set; get; }

    public override void _Ready()
    {
        if (!labIndicies.Add(Index))
            GD.PrintErr($"Lab: {Name} has duplicate Index");
        if (Index == 0)
            Lab.CurrentLab = this;
    }

    private void Toggle(bool state)
    {
        TopGate.GetNode<CollisionShape3D>()!.Disabled = state;
        BottomGate.GetNode<CollisionShape3D>()!.Disabled = state;
        TopGate.Visible = !state;
        BottomGate.Visible = !state;
    }
}
