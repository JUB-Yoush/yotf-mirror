using System.Collections.Generic;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Firecracker : RigidBody3D, IMakeNoise, IGiveLight
{
    public override void _Notification(int what) => this.Notify(what);

    static readonly PackedScene Packed = GD.Load<PackedScene>(
        "res://src/entities/gadgets/firecracker/firecracker.tscn"
    );

    public static Firecracker New(Node parent, Vec3 impulse)
    {
        var firecracker = Packed.Instantiate<Firecracker>();
        parent.AddChild(firecracker);
        firecracker.ApplyImpulse(impulse);
        return firecracker;
    }

    [Node]
    public required GpuParticles3D Particles { set; get; }

    [Node]
    public required AudioStreamPlayer3D AudioPlayer { set; get; }

    [Node]
    public required OmniLight3D OmniLight { set; get; }

    public AudioStreamPlayer3D NoiseSource => AudioPlayer;

    public Light3D LightSource => OmniLight;

    [Node]
    public required Area3D LightArea { set; get; }

    [Export]
    float waitTime = 3f;

    [Export]
    float lightEnergy = 5f;

    [Export]
    float lightRange = 10f;

    [Export]
    float lifetime = 5f;

    private List<IPhotographable> trackedSubjects = [];

    Vec3 InitialVelocity;

    bool ran = false;

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
                IMakeNoise.MakeNoise(this, 0, Sfx.Firecracker);
            },
            waitTime
        );
        tween.TweenFn<float>((value) => OmniLight.LightEnergy = value, 0, lightEnergy, 1f, true);
        tween.TweenFn<float>((value) => OmniLight.OmniRange = value, 0, lightEnergy, 1f, true);
        tween.Fn(
            () =>
            {
                AudioPlayer.Stop();
                ran = true;
            },
            lifetime
        );

        LightArea.AreaEntered += OnReceivedObject;
        LightArea.AreaExited += OnRemovedObject;
    }

    public void OnReceivedObject(Node3D body)
    {
        GD.Print($"Firecracker detected {body}");
        if (body is IPhotographable p && !p.IsModifier)
        {
            trackedSubjects.Add(p);
            p.OnReceivedLight(this);
            GD.Print($"Firecracker added light to {p}");
        }
    }

    public void OnRemovedObject(Node3D body)
    {
        GD.Print($"Firecracker lost track of {body}");
        if (body is IPhotographable p)
        {
            trackedSubjects.Remove(p);
            p.OnRemovedLight(this);
            GD.Print($"Firecracker removed light from {p}");
        }
    }

    public override void _ExitTree()
    {
        foreach (var p in trackedSubjects)
            p.OnRemovedLight(this);
        trackedSubjects.Clear();
    }

    public override void _Process(double delta)
    {
        Particles.GlobalPosition = GlobalPosition;

        if (ran == true)
        {
            QueueFree();
        }
    }
}
