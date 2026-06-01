using System;
using System.Linq;
using Godot;

namespace Yotf;

[Tool]
[GlobalClass]
public partial class NavNode : Node3D
{
    [Export]
    public NavNode[] neighbors = [];

    public void AddNeighbor(NavNode nei)
    {
        if (!neighbors.Contains<NavNode>(nei))
        {
            var listver = neighbors.ToList<NavNode>();
            listver.Add(nei);
            neighbors = [.. listver];
        }

        //ensure bi-directionality
        if (!nei.neighbors.Contains<NavNode>(this))
            nei.AddNeighbor(this);
    }
}
