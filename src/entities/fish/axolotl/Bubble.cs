using System;
using Godot;

namespace Yotf;

public partial class Bubble : CharacterBody3D
{
    public static readonly PackedScene Packed = GD.Load<PackedScene>(
        "res://src/entities/fish/axolotl/bubble.tscn"
    );

    private float shotSpeed = 1f;

    private float riseSpeed = 1f;

    private float deceleration = 0.1f;

    private float lifetime = 5f;

    private IBubbleable? capturedNode = null;

    public Vector3 SpawnDir = Vector3.Zero;

    public static Bubble New(Vector3 spawnDir, float shotSpeed, float riseSpeed, float deceleration)
    {
        var bubble = Packed.Instantiate<Bubble>();
        bubble.SpawnDir = spawnDir;
        bubble.shotSpeed = shotSpeed;
        bubble.riseSpeed = riseSpeed;
        bubble.deceleration = deceleration;
        return bubble;
    }

    public override void _Ready()
    {
        TopLevel = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        lifetime -= (float)delta;
        if (lifetime <= 0 && capturedNode == null)
        {
            QueueFree();
        }
        Velocity = SpawnDir * shotSpeed + new Vector3(0, riseSpeed, 0);
        shotSpeed = Mathf.Lerp(shotSpeed, 0, deceleration);
        MoveAndSlide();
    }
}
