using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Lab : Node3D
{
    public override void _Notification(int what) => this.Notify(what);

    public static Action<Lab>? CurrentLabUpdated;

    public static readonly Dictionary<int, Lab> Map = [];

    public static Lab? CurrentLab
    {
        set
        {
            field?.Toggle(false);
            field = value;
            field!.Toggle(true);
            CurrentLabUpdated?.Invoke(field);
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

    [Node]
    public required Marker3D PlayerSpawn { set; get; }

    public override void _Ready()
    {
        Debug.Assert(Index != -1, $"Lab {Name} not given index");
        if (!Map.TryAdd(Index, this))
            GD.PrintErr($"Lab: {Name} and {Map[Index].Name} have duplicate Indicies");
        if (Index == 0)
            Lab.CurrentLab = this;
    }

    private void Toggle(bool state)
    {
        TopGate.GetNode<CollisionShape3D>()!.Disabled = state;
        BottomGate.GetNode<CollisionShape3D>()!.Disabled = state;
        TopGate.Visible = !state;
        BottomGate.Visible = !state;
        foreach (var fish in this.SceneRoot().GetNodes<Fish>())
        {
            if (fish.LabLayer == this)
            {
                fish.ProcessMode = ProcessModeEnum.Inherit;
            }
            else
            {
                fish.ProcessMode = ProcessModeEnum.Disabled;
            }
        }
        this.SceneRoot().GetNode<Player>()!.Stats.TotalGalleryScore = 0;
    }

    public static Lab? GetLabByIndex(int index)
    {
        if (Map.TryGetValue(index, out var lab))
            return lab;
        return null;
    }
}
