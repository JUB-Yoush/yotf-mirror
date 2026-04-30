using System;
using System.Runtime.CompilerServices;
using Godot;

public static class NodeExt
{
    public static Node GetSceneRoot(this Node node) => node.GetTree().CurrentScene;
}
