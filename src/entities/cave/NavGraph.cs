using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace Yotf;

/// <summary>
/// A graph of 3D nodes, used for fish pathfinding throughout the level.
/// </summary>
[Tool]
[GlobalClass]
public partial class NavGraph : Node3D
{
    [ExportToolButton("Verify Edge BiDirectionality")]
    public Callable VerifyBtn => Callable.From(VerifyBiDirectionality);

    public static readonly Material EdgeMaterial = GD.Load<Material>(
        "res://assets/materials/node_edge_material.tres"
    );

    public ImmediateMesh immMesh = new();
    public MeshInstance3D EdgesView = null!;
    public NavNode[] NavNodes
    {
        private set;
        get
        {
            field = GetNavNodes();
            return field;
        }
    } = null;

    public override void _Ready()
    {
        VerifyBiDirectionality();
        EdgesView = this.GetNode<MeshInstance3D>()!;
        EdgesView.MaterialOverride = EdgeMaterial;

        // if (!Engine.IsEditorHint())
        //     Visible = false;
    }

    private void VerifyBiDirectionality()
    {
        foreach (var node in GetNavNodes())
        {
            for (int i = 0; i < node.neighbors.Length; i++)
            {
                if (node.neighbors[i] == null)
                {
                    GD.PrintErr($"Nav Graph Node {node.Name} has null neighbor at position {i}");
                }
                node.AddNeighbor(node.neighbors[i]);
            }
        }
    }

    public override void _Process(double delta)
    {
        // if (!Engine.IsEditorHint())
        //     return;

        NavNodes = GetNavNodes();
        bool anyConnections = false;
        if (NavNodes.Length < 2)
            return;

        immMesh.ClearSurfaces();
        immMesh = new ImmediateMesh();

        immMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

        HashSet<NavNode> visited = [];
        Queue<NavNode> toDraw = [];

        toDraw.Enqueue(NavNodes[0]);

        while (toDraw.Count > 0)
        {
            var curr = toDraw.Dequeue();

            if (!visited.Add(curr))
                continue;

            foreach (var nei in curr.neighbors)
            {
                toDraw.Enqueue(nei);
                immMesh.SurfaceAddVertex(curr.Position);
                immMesh.SurfaceAddVertex(nei.Position);
                anyConnections = true;
            }
        }
        if (!anyConnections)
        {
            immMesh.SurfaceAddVertex(Vector3.Zero);
            immMesh.SurfaceAddVertex(Vector3.One);
        }

        immMesh.SurfaceEnd();

        EdgesView.Mesh = immMesh;
    }

    private NavNode[] GetNavNodes()
    {
        List<NavNode> result = [];
        foreach (var node in GetChildren())
        {
            if (node is NavNode nav)
                result.Add(nav);
        }
        return [.. result];
    }

    public NavNode NodeClosestTo(Vector3 pos, NavNode? currentToAvoid = null)
    {
        // TODO(j) shoot a raycast to make sure it's not behind a wall or somthn
        (NavNode?, float) result = (null, float.MaxValue);
        foreach (var node in GetNavNodes())
        {
            if (
                (node.GlobalPosition - pos).LengthSquared() <= result.Item2
                && (currentToAvoid == null || currentToAvoid != node)
            )
            {
                result = (node, (node.GlobalPosition - pos).LengthSquared());
            }
        }
        return result.Item1!;
    }

    public NavNode? NodeAwayFrom(Vector3 pos, Vector3 awayFrom)
    {
        (NavNode?, float) record = (null, float.MaxValue);
        foreach (var node in GetNavNodes())
        {
            if (
                (node.GlobalPosition - pos).LengthSquared() <= record.Item2
                && (
                    (pos - awayFrom).LengthSquared()
                    <= (node.GlobalPosition - awayFrom).LengthSquared()
                )
            )
            {
                record = (node, (node.GlobalPosition - pos).LengthSquared());
            }
        }
        return record.Item1!;
    }

    public NavNode RandomNode(NavNode? notThisOne = null)
    {
        var next = NavNodes[GD.RandRange(0, NavNodes.Length - 1)];
        while (notThisOne != null && next == notThisOne)
        {
            next = NavNodes[GD.RandRange(0, NavNodes.Length - 1)];
        }
        return next;
    }
}
