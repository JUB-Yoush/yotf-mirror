using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace Yotf;

/*
 * fish wander navigation.
 * fish use this graph to traverse the level. fish are not always traveling based on this graph. only when they are wandering or pathfinding ig
 * when a fish wants to wander:
 * pick the nearest node to you, that is your current node
 * travel towards your current node until you reach it
 * pick one of it's neighbours that is within your travel range (2nd pass)
   travel range is a stat per fish, the distance from it's current "room" + it's range
    the closest room to a fish is it's wander room
* travel to that neighbour
*/

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
            // if(node == null){
            //    GD.PrintErr("Null node found in ")
            // }
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
                //Debug.Assert(nei != null);
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
        List<NavNode> res = [];
        foreach (var node in GetChildren())
        {
            if (node is NavNode nav)
                res.Add(nav);
        }
        res.ForEach(
            (node) =>
            {
                if (node == null)
                {
                    Log.PrintLn("graph node is null what the flip");
                }
            }
        );
        return [.. res];
    }

    public NavNode NodeClosestTo(Vector3 pos)
    {
        // TODO(j) shoot a raycast to make sure it's not behind a wall or somthn
        (NavNode?, float) record = (null, float.MaxValue);
        foreach (var node in GetNavNodes())
        {
            if ((node.GlobalPosition - pos).LengthSquared() <= record.Item2)
            {
                record = (node, (node.GlobalPosition - pos).LengthSquared());
            }
        }
        return record.Item1!;
    }
}
