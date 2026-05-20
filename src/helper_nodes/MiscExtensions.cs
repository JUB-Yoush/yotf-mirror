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
        public Node SceneRoot() => node.GetTree().CurrentScene;

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

        /// <summary>
        /// Loops over scene tree to find the first child of matching type.
        /// </summary>
        public T? GetNode<T>(bool includeInternal = false, bool mustExist = false)
            where T : Node
        {
            foreach (var child in node.GetChildren(includeInternal))
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

        /// <summary>
        /// Loops over scene tree to find all children of matching type.
        /// </summary>
        public T[] GetNodes<T>(bool includeInternal = false)
            where T : Node
        {
            var res = new List<T>();
            foreach (var child in node.GetChildren(includeInternal))
            {
                if (child is T t)
                    res.Add(t);
            }
            return [.. res];
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
    extension<T>(List<T> list)
    {
        public T Pop()
        {
            T val = list[^1];
            list.RemoveAt(list.Count - 1);
            return val;
        }

        public T PopAt(int i)
        {
            T val = list[i];
            list.RemoveAt(i);
            return val;
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
