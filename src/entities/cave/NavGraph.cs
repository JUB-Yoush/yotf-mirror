using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

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
    public NavNode[] navNodes = null!;

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
                node.AddNeighbor(node.neighbors[i]);
        }
    }

    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint())
            return;
        navNodes = GetNavNodes();

        immMesh.ClearSurfaces();
        immMesh = new ImmediateMesh();

        immMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

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
