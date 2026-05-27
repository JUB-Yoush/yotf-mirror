using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;

namespace Yotf;

public static class MiscExt
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 V3Lerp(Vector3 from, Vector3 to, float weight)
    {
        return new(
            Mathf.Lerp(from.X, to.X, weight),
            Mathf.Lerp(from.Y, to.Y, weight),
            Mathf.Lerp(from.Z, to.Z, weight)
        );
    }

    extension(Tween tween)
    {
        public void Fn(
            Action action,
            float delay = 0,
            bool parallel = false,
            bool resetIfRunning = false
        )
        {
            if (resetIfRunning && tween.IsRunning())
                tween.Stop();

            if (parallel)
            {
                tween.Parallel().TweenCallback(Callable.From(action)).SetDelay(delay);
            }
            else
            {
                tween.TweenCallback(Callable.From(action)).SetDelay(delay);
            }
        }

        public void LerpProperty(
            Node node,
            StringName property,
            Variant value,
            float time,
            bool parallel = false
        )
        {
            tween.TweenProperty(node, property.ToString(), value, time);
        }

        public void TweenFn<T>(Action<T> action, T from, T to, float time, bool parallel = false)
            where T : struct
        {
            if (parallel)
            {
                tween
                    .Parallel()
                    .TweenMethod(Callable.From(action), Variant.From(from), Variant.From(to), time);
            }
            else
            {
                tween.TweenMethod(
                    Callable.From(action),
                    Variant.From(from),
                    Variant.From(to),
                    time
                );
            }
        }

        public SignalAwaiter Done(Node node) => node.ToSignal(tween, Tween.SignalName.Finished);

        public SignalAwaiter Done() => tween.ToSignal(tween, Tween.SignalName.Finished);
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
        public T? GetNode<T>(bool includeInternal = false)
            where T : class
        {
            foreach (var child in node.GetChildren(includeInternal))
            {
                if (child is T t)
                    return t;
            }
            return null;
        }

        /// <summary>
        /// Loops over scene tree to find all children of matching type.
        /// </summary>
        public T[] GetNodes<T>(bool includeInternal = false)
            where T : class
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T PopLast() => list.Pop(^1);

        public T Pop(Index i)
        {
            var offset = i.GetOffset(list.Count);
            T val = list[offset];
            list.RemoveAt(offset);
            return val;
        }
    }

    extension(Vector3 vec)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 XY() => new(vec.X, vec.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 XZ() => new(vec.X, vec.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 YZ() => new(vec.Y, vec.Z);
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
