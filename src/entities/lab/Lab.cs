using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Lab : Node3D
{
    public override void _Notification(int what) => this.Notify(what);

    public static Action<Lab>? CurrentLabUpdated;

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

    [Node]
    public required PhotoTerminal PhotoTerminal { set; get; }

    [Node]
    public required ShopKiosk ShopKiosk { set; get; }

    public override void _Ready()
    {
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
