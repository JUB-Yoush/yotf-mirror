using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Godot;

public static class NodeExt
{
    public static Node GetSceneRoot(this Node node) => node.GetTree().CurrentScene;

    public static List<Node> GetAllChildren(this Node root)
    {
        Queue<Node> queue = [];
        List<Node> res = [];
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            res.Add(node);
            foreach (var child in node.GetChildren(true))
            {
                queue.Enqueue(child);
            }
        }
        return res;
    }

    // public static IEnumerable<T> QueryInterfaces<T>(IEnumerable<T> nodes, Type[] interfaces)
    // {
    //     foreach (var node in nodes)
    //     {
    //         if (node is null)
    //             continue;
    //         var ok = true;
    //         for (int i = 0; i < interfaces.Length; i++)
    //         {
    //             if (!interfaces[i].IsInstanceOfType(node))
    //             {
    //                 ok = false;
    //                 break;
    //             }
    //         }
    //         if (ok)
    //             yield return node;
    //     }
    // }
}
