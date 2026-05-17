using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;

namespace Yotf;

public static class MiscExtensions
{
    extension(Tween tween)
    {
        public void Fn(Action action, float delay = 0, bool parallel = false)
        {
            if (parallel)
            {
                tween.Parallel().TweenCallback(Callable.From(action)).SetDelay(delay);
            }
            else
            {
                tween.TweenCallback(Callable.From(action)).SetDelay(delay);
            }
        }

        public void TweenProperty(Node node, StringName property, Variant value, float time)
        {
            tween.TweenProperty(node, property.ToString(), value, time);
        }

        public void TweenFn<T>(Action<T> action, T from, T to, float time)
            where T : struct
        {
            tween.TweenMethod(Callable.From(action), Variant.From(from), Variant.From(to), time);
        }
    }

    extension(Node node)
    {
        public Node GetSceneRoot() => node.GetTree().CurrentScene;

        public void RemoveAllChildren()
        {
            foreach (var child in node.GetChildren())
                child.QueueFree();
        }

        public List<Node> GetChildrenRecursive()
        {
            Queue<Node> queue = [];
            List<Node> res = [];
            queue.Enqueue(node);
            while (queue.Count > 0)
            {
                var child = queue.Dequeue();
                res.Add(child);
                foreach (var grandkid in child.GetChildren(true))
                {
                    queue.Enqueue(grandkid);
                }
            }
            return res;
        }

        public T? GetChildOfType<T>(bool mustExist = false)
            where T : Node
        {
            foreach (var child in node.GetChildren())
            {
                if (child is T t)
                    return t;
            }
            Debug.Assert(
                !mustExist,
                $"Node that was supposed to be child of {node.Name} here wasnt"
            );
            return null;
        }

        public async Task WaitUntil(Func<bool> condition)
        {
            while (!condition())
                await node.ToSignal(node.GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        public async Task WaitUntil(bool condition)
        {
            while (!condition)
                await node.ToSignal(node.GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }

    static void TryMakeDir(string path)
    {
        using var dir = DirAccess.Open(path);
        if (dir == null)
        {
            DirAccess.MakeDirAbsolute(path);
        }
    }
}
