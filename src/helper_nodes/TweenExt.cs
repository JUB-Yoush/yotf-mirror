using System;
using System.Runtime.CompilerServices;
using Godot;

public static class TweenExt
{
    public static void Call(this Tween tween, Action action, float delay = 0, bool parallel = false)
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
