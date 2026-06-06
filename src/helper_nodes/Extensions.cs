using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;

namespace Yotf;

public static class GDExt
{
    extension(Tween tween)
    {
        public CallbackTweener Fn(
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
                return tween.Parallel().TweenCallback(Callable.From(action)).SetDelay(delay);
            }
            else
            {
                return tween.TweenCallback(Callable.From(action)).SetDelay(delay);
            }
        }

        public PropertyTweener AnimateProperty(
            Node node,
            StringName property,
            Variant value,
            float time,
            bool parallel = false
        )
        {
            if (parallel)
            {
                return tween.Parallel().TweenProperty(node, property.ToString(), value, time);
            }
            else
            {
                return tween.TweenProperty(node, property.ToString(), value, time);
            }
        }

        public MethodTweener TweenFn<T>(
            Action<T> action,
            T from,
            T to,
            float time,
            bool parallel = false
        )
            where T : struct
        {
            if (parallel)
            {
                return tween
                    .Parallel()
                    .TweenMethod(Callable.From(action), Variant.From(from), Variant.From(to), time);
            }
            else
            {
                return tween.TweenMethod(
                    Callable.From(action),
                    Variant.From(from),
                    Variant.From(to),
                    time
                );
            }
        }

        public SignalAwaiter Done() => tween.ToSignal(tween, Tween.SignalName.Finished);
    }

    extension(Label label)
    {
        public void RenderGradually(string msg, float speed)
        {
            label.Text = "";
            var i = 0;
            var timer = new Timer();
            label.AddChild(timer);
            timer.WaitTime = speed;
            timer.OneShot = false;
            timer.Timeout += () =>
            {
                i++;
                label.Text = msg[..Math.Min(i, msg.Length)];
                if (i >= msg.Length)
                {
                    timer.Stop();
                    timer.QueueFree();
                }
            };
            timer.Start();
        }
    }
    extension(GodotObject obj)
    {
        public bool IsValid() => GodotObject.IsInstanceValid(obj);
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

        public void DeferFree()
        {
            node.CallDeferred(Node.MethodName.QueueFree);
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

        public Timer TimedEvent(Action action, float delay)
        {
            var timer = new Timer();
            node.AddChild(timer);
            timer.WaitTime = delay;
            timer.Timeout += action;
            timer.Start();
            timer.Timeout += () => timer.QueueFree();
            return timer;
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

    extension<T>(T[] array)
    {
        public void ForEach(Action<T> action)
        {
            Array.ForEach<T>(array, action);
        }
    }

    extension(Vector3 vec)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec2 XY() => new(vec.X, vec.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec2 XZ() => new(vec.X, vec.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec2 YZ() => new(vec.Y, vec.Z);
    }

    extension(ref Vector3 vec)
    {
        public void Change(float x = 0, float y = 0, float z = 0)
        {
            vec.X += x;
            vec.Y += y;
            vec.Z += z;
        }

        public void Set(float x = 0, float y = 0, float z = 0)
        {
            vec.X = x;
            vec.Y = y;
            vec.Z = z;
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

    // public static Vec3 RandomUnitVec(float x = 0, float y = 0, float z = 0)
    // {
    //     x = x != 0 ? x : GD.Randf();
    //     y = y != 0 ? y : GD.Randf();
    //     z = z != 0 ? z : GD.Randf();
    //     return new Vec3(x, y, z).Normalized();
    // }

    public static Dictionary<StringName, T> LoadFromFolder<T>(string path)
        where T : Resource
    {
        var result = new Dictionary<StringName, T>();
        using var dir = DirAccess.Open(path);
        if (dir == null)
        {
            GD.PrintErr(
                $"An error occurred when trying to access the path:{path}: {DirAccess.GetOpenError()}"
            );
            return [];
        }

        dir.ListDirBegin();
        string fileName = dir.GetNext();
        while (fileName != "")
        {
            if (!dir.CurrentIsDir())
            {
                var res = GD.Load<T>($"{path}/{fileName}");
                result.Add(fileName, res);
            }
            else
            {
                GD.PushWarning($"folder: {fileName} found in {path}. Ignoring contents");
            }
            fileName = dir.GetNext();
        }
        return result;
    }
}
