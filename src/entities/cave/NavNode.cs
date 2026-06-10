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
            if (!Engine.IsEditorHint())
                return;
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
            if (!Engine.IsEditorHint())
                return;
            if (!value)
            {
                return;
            }
            if (!joinMutex)
            {
                joinMutex = true;

                var selected = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
                if (selected.Count < 2)
                {
                    GD.PrintErr("Less than 2 nav nodes selected");
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

    private static void JoinNodes(Array<Node> nodes)
    {
        Array<NavNode> navNodes = [];
        foreach (var node in nodes)
        {
            if (node is not NavNode)
            {
                GD.PrintErr("Non nav node passed into Join Function");
                return;
            }
            else
            {
                navNodes.Add((NavNode)node);
            }
        }

        foreach (var navNode in navNodes)
        {
            foreach (var otherNode in navNodes)
            {
                if (navNode == otherNode)
                    continue;

                if (!navNode.neighbors.Contains(otherNode))
                    navNode.neighbors.Add(otherNode);

                if (!otherNode.neighbors.Contains(navNode))
                    otherNode.neighbors.Add(navNode);
            }
        }
    }

    [Export]
    public bool NewPoint
    {
        set
        {
            if (!Engine.IsEditorHint())
                return;
            if (value)
                AddNewPoint();
        }
        get;
    }

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
        //newNode.Room = this.Room;
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

        //newNode.Room = this.Room;

        var dist = Position - other.Position;
        newNode.Position = other.Position + (dist / 2);

        EditorInterface.Singleton.MarkSceneAsUnsaved();
    }

    public override void _Ready()
    {
        if (!Engine.IsEditorHint())
        {
            Visible = false;
            Debug.Assert(Room != null, $"No room provided for nav vertex {Name}");
        }
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

    public void ClearMissingNeighbours()
    {
        for (int i = 0; i < neighbors.Count; i++)
        {
            var nei = neighbors[i];
            if (!nei.IsInsideTree())
            {
                neighbors.Remove(nei);
            }
        }
    }

    public NavNode RandomNeighbor() => neighbors[GD.RandRange(0, neighbors.Count - 1)];
}
