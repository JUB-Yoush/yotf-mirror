using System;
using Godot;

namespace Yotf;

[Tool]
[GlobalClass]
public partial class NavNode : Node3D
{
    [Export]
    public NavNode[] neighbors = [];
}
