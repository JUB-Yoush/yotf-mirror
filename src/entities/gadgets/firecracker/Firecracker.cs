using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Firecracker : RigidBody3D, IMakeNoise
{
    public override void _Notification(int what) => this.Notify(what);

    static readonly PackedScene Packed = GD.Load<PackedScene>(
        "res://src/entities/gadgets/firecracker/firecracker.tscn"
    );

    public static Firecracker New(Vec3 initalVelocity)
    {
        var firecracker = Packed.Instantiate<Firecracker>();
        firecracker.InitialVelocity = initalVelocity;
        firecracker.LinearVelocity = initalVelocity;
        return firecracker;
    }

    [Node]
    public required GpuParticles3D Particles { set; get; }

    [Node]
    public required AudioStreamPlayer3D AudioPlayer { set; get; }

    [Node]
    public required OmniLight3D OmniLight { set; get; }

    public AudioStreamPlayer3D NoiseSource => AudioPlayer;

    [Export]
    float waitTime = 3f;

    [Export]
    float lightEnergy = 5f;

    [Export]
    float lightRange = 10f;

    [Export]
    float lifetime = 5f;

    Vec3 InitialVelocity;

    bool on = false;

    public override void _Ready()
    {
        Particles.TopLevel = true;
        Particles.Lifetime = lifetime;

        OmniLight.LightEnergy = 0;
        OmniLight.OmniRange = 0;

        var tween = CreateTween();
        tween.Fn(
            () =>
            {
                Particles.Emitting = true;
                IMakeNoise.MakeNoise(this, 0, SFX.Firecracker);
            },
            waitTime
        );
        tween.TweenFn<float>((value) => OmniLight.LightEnergy = value, 0, lightEnergy, 1f, true);
        tween.TweenFn<float>((value) => OmniLight.OmniRange = value, 0, lightEnergy, 1f, true);
        tween.Fn(
            () =>
            {
                AudioPlayer.Stop();
                this.DeferFree();
            },
            lifetime
        );
    }

    public override void _Process(double delta)
    {
        Particles.GlobalPosition = GlobalPosition;
    }
}
