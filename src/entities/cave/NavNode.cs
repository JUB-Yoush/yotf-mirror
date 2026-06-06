using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Yotf;

/// <summary>
/// A node of a graph, used for fish pathfinding throughout the level.
/// </summary>
[Tool]
[GlobalClass]
public partial class NavNode : Node3D
{
    static readonly PackedScene Packed = GD.Load<PackedScene>(
        "res://src/entities/cave/nav_node.tscn"
    );

    [Export]
    public Array<NavNode> neighbors = [];

    [Export]
    public required Marker3D Room;

    [Export]
    public bool bisect
    {
        set
        {
            if (!value)
            {
                return;
            }
            if (!bisectMutex)
            {
                bisectMutex = true;

                var selected = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
                if (selected.Count != 2)
                {
                    GD.PrintErr("More or less than 2 nav nodes selected");
                    return;
                }
                if (selected[0] is NavNode navNode && selected[1] is NavNode other)
                {
                    navNode.Bisect(other);
                }
                else
                {
                    GD.PrintErr("Non Nav Node selected");
                }
            }
            else
            {
                bisectMutex = false;
            }
        }
        get;
    }

    [Export]
    public bool Join
    {
        set
        {
            if (!value)
            {
                return;
            }
            if (!joinMutex)
            {
                joinMutex = true;

                var selected = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
                if (selected.Count != 2)
                {
                    GD.PrintErr("More or less than 2 nav nodes selected");
                    return;
                }
                JoinNodes(selected);
            }
            else
            {
                joinMutex = false;
            }
        }
        get;
    }

    private static void JoinNodes(Array<Node> nodes) { }

    [Export]
    public bool NewPoint
    {
        set
        {
            if (value)
                AddNewPoint();
        }
        get;
    }

    //public Callable BallsBtn => Callable.From(Balls);

    static bool bisectMutex = false;
    static bool joinMutex = false;
    static int newPointMutex = 0;

    void AddNewPoint()
    {
        var navNodeCount = GetParent().GetChildCount();
        var newNode = Packed.Instantiate<NavNode>();
        newNode.Name = $"NavNode{(navNodeCount)}";
        GetParent().AddChild(newNode);
        newNode.Owner = GetTree().EditedSceneRoot;
        newNode.Position = Position + Vec3.Forward;
        newNode.neighbors.Add(this);
        this.neighbors.Add(newNode);

        EditorInterface.Singleton.MarkSceneAsUnsaved();
    }

    void Bisect(NavNode other)
    {
        var navNodeCount = GetParent().GetChildCount();
        var newNode = Packed.Instantiate<NavNode>();
        newNode.Name = $"NavNode{(navNodeCount)}";
        GetParent().AddChild(newNode);
        newNode.Owner = GetTree().EditedSceneRoot;
        newNode.neighbors = [this, other];
        neighbors.Remove(other);
        neighbors.Add(newNode);
        other.neighbors.Remove(this);
        other.neighbors.Add(newNode);

        var dist = Position - other.Position;
        newNode.Position = other.Position + (dist / 2);

        EditorInterface.Singleton.MarkSceneAsUnsaved();
    }

    public void AddNeighbor(NavNode nei)
    {
        if (!neighbors.Contains<NavNode>(nei))
        {
            var listver = neighbors.ToList<NavNode>();
            listver.Add(nei);
            neighbors = [.. listver];
        }

        //ensure bi-directionality of graph
        if (!nei.neighbors.Contains<NavNode>(this))
            nei.AddNeighbor(this);
    }

    public NavNode RandomNeighbor() => neighbors[GD.RandRange(0, neighbors.Count - 1)];
}
