using System.Collections.Generic;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class GlowingCoral : Node3D, IGiveLight
{
    public override void _Notification(int what) => this.Notify(what);

    [Node]
    public required Area3D LightArea { set; get; }

    [Node]
    public required RayCast3D LightRay { set; get; }

    public Light3D? LightSource => null;

    [Export]
    public float LightEnergy { get; set; } = 1f;

    [Export]
    public float EffectiveRange { get; set; } = 10f;

    private readonly List<IPhotographable> trackedSubjects = [];

    public override void _Ready()
    {
        LightArea.BodyEntered += OnReceivedObject;
        LightArea.BodyExited += OnRemovedObject;
    }

    public void OnReceivedObject(Node3D body)
    {
        if (body is IPhotographable p && !p.IsModifier)
        {
            trackedSubjects.Add(p);
            p.OnReceivedLight(this);
        }
    }

    public void OnRemovedObject(Node3D body)
    {
        if (body is IPhotographable p)
        {
            trackedSubjects.Remove(p);
            p.OnRemovedLight(this);
        }
    }

    public override void _ExitTree()
    {
        foreach (var p in trackedSubjects)
            p.OnRemovedLight(this);
        trackedSubjects.Clear();
    }
}
