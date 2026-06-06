using System;
using System.Diagnostics;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Bubble : CharacterBody3D
{
    public override void _Notification(int what) => this.Notify(what);

    public static readonly PackedScene Packed = GD.Load<PackedScene>(
        "res://src/entities/fish/axolotl/bubble.tscn"
    );

    [Node]
    public required Sprite3D Sprite { set; get; }

    [Node]
    public required MeshInstance3D Mesh { set; get; }

    [Node]
    public required Area3D BubbleableArea { set; get; }

    private float shotSpeed = 1f;

    private float riseSpeed = 1f;

    private float deceleration = 0.1f;

    private float lifetime = 5f;

    private const float bubbledLifetime = 5f;

    private IBubbleable? capturedNode = null;

    private Axolotl origin = null!;

    public Vec3 SpawnDir = Vec3.Zero;

    public static Bubble New(
        Axolotl origin,
        Vec3 spawnDir,
        float shotSpeed,
        float riseSpeed,
        float deceleration
    )
    {
        var bubble = Packed.Instantiate<Bubble>();
        bubble.origin = origin;
        bubble.SpawnDir = spawnDir;
        bubble.shotSpeed = shotSpeed;
        bubble.riseSpeed = riseSpeed;
        bubble.deceleration = deceleration;
        return bubble;
    }

    public override void _Ready()
    {
        BubbleableArea.BodyEntered += OnBodyEntered;
        TopLevel = true;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is IBubbleable bubbleable && bubbleable.CanBeBubbled)
        {
            PutInBubble(bubbleable);
        }
    }

    void PutInBubble(IBubbleable bubbleable)
    {
        if (bubbleable.CanBeBubbled && bubbleable.BubbleJail == null && origin != bubbleable)
        {
            Mesh.Mesh = bubbleable.Mesh;
            Mesh.Scale = new(bubbleable.MeshScale, bubbleable.MeshScale, bubbleable.MeshScale);
            bubbleable.BubbleJail = this;
            bubbleable.PutInBubble();
            capturedNode = bubbleable;
        }
    }

    void FreeCapturedNode()
    {
        capturedNode?.FreeFromBubble();
        capturedNode?.BubbleJail = null;
        this.DeferFree();
    }

    public override void _PhysicsProcess(double delta)
    {
        lifetime -= (float)delta;
        if (lifetime <= 0)
        {
            FreeCapturedNode();
        }
        Velocity = SpawnDir * shotSpeed + new Vec3(0, riseSpeed, 0);
        shotSpeed = Mathf.Lerp(shotSpeed, 0, deceleration);
        MoveAndSlide();
    }
}
