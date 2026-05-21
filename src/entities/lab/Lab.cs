using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Lab : Node3D
{
    public override void _Notification(int what) => this.Notify(what);

    public static Lab? CurrentLab = null;

    [Node]
    public required StaticBody3D TopGate { set; get; }

    [Node]
    public required StaticBody3D BottomGate { set; get; }

    [Export]
    public int Index = 0;

    [Export]
    public int requiredGalleryScore;

    [Node]
    public required PhotoTerminal PhotoTerminal { set; get; }

    [Node]
    public required ShopKiosk ShopKiosk { set; get; }

    public override void _Ready()
    {
        //CurrentLab ??= labIndex == 0 ? this : null;
        if (Index == 0)
        {
            Log.PrintLn(Index);
            SetCurrentLab(this);
        }
    }

    internal static void SetCurrentLab(Lab lab)
    {
        var prev = Lab.CurrentLab;
        prev?.TopGate.GetNode<CollisionShape3D>()!.Disabled = false;
        prev?.BottomGate.GetNode<CollisionShape3D>()!.Disabled = false;
        prev?.TopGate.Visible = true;
        prev?.BottomGate.Visible = true;

        Lab.CurrentLab = lab;
        lab.TopGate.GetNode<CollisionShape3D>()!.Disabled = true;
        lab.BottomGate.GetNode<CollisionShape3D>()!.Disabled = true;
        lab.TopGate.Visible = false;
        lab.BottomGate.Visible = false;
    }
}
