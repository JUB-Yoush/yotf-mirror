using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;

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
    }

    extension(Node node)
    {
        public Node GetSceneRoot() => node.GetTree().CurrentScene;

        public List<Node> GetAllChildren()
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
    }
}
