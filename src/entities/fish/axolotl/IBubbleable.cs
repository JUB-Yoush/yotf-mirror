using System;
using Godot;

namespace Yotf;

// on any Node3D
public interface IBubbleable
{
    public Node3D Node3D { get; }
    public bool InBubble { set; get; }
    public Bubble Bubble { set; get; }
}
