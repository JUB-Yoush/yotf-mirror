using System;
using Godot;

namespace Yotf;

public class MathExt
{
    public static float Lerp(float a, float b, float percent) => (a + (b - a) * percent);
}
