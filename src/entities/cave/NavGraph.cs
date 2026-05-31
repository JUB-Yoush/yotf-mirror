using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

[Tool]
[GlobalClass]
public partial class NavGraph : Node3D
{
    public static readonly Material EdgeMaterial = GD.Load<Material>(
        "res://assets/materials/node_edge_material.tres"
    );

    public ImmediateMesh immMesh = new();
    public MeshInstance3D EdgesView = null!;
    public NavNode[] navNodes = null!;

    public override void _Ready()
    {
        //TopLevel = true;
        //GlobalPosition = Vector3.Zero;
        EdgesView = this.GetNode<MeshInstance3D>()!;
        //navNodes = this.GetNodes<NavNode>();
        Log.PrintLn("in the editor wooop");
        EdgesView.MaterialOverride = EdgeMaterial;
    }

    public override void _Process(double delta)
    {
        navNodes = GetNavNodes();
        var navNode = GetNode<NavNode>("NavNode");
        var navNode2 = GetNode<NavNode>("NavNode2");

        immMesh.ClearSurfaces();
        immMesh = new ImmediateMesh();

        immMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

        // immMesh.SurfaceAddVertex(navNodes[0].Position);
        // immMesh.SurfaceAddVertex(navNodes[1].Position);
        //
        // immMesh.SurfaceAddVertex(Vector3.Zero);
        // immMesh.SurfaceAddVertex(Vector3.Up);

        HashSet<NavNode> visited = [];
        Queue<NavNode> toDraw = [];

        toDraw.Enqueue(navNodes[0]);

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
            }
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
        return [.. res];
    }
}
