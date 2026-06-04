using System;
using System.Diagnostics;
using System.Linq;
using Godot;

namespace Yotf;

/// <summary>
/// A node of a graph, used for fish pathfinding throughout the level.
/// </summary>
[Tool]
[GlobalClass]
public partial class NavNode : Node3D
{
    [Export]
    public NavNode[] neighbors = [];

    [Export]
    public required Marker3D Room;

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

    public NavNode RandomNeighbor() => neighbors[GD.RandRange(0, neighbors.Length - 1)];
}
