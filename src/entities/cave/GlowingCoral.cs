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

        // set the effective range to match radius of the light area
        var collisionShape  = LightArea.GetNode<CollisionShape3D>("CollisionShape3D");
        if (collisionShape != null && collisionShape.Shape is SphereShape3D sphereShape)
        {
            EffectiveRange = sphereShape.Radius * GlobalTransform.Basis.Scale.X; // account for scaling
        } 
    }

    public void OnReceivedObject(Node3D body)
    {
        if (body is IPhotographable p && !p.IsModifier)
        {
            trackedSubjects.Add(p);
            p.OnReceivedLight(this);
            GD.Print($"GlowingCoral added light to {body.Name}");
        }
    }

    public void OnRemovedObject(Node3D body)
    {
        if (body is IPhotographable p)
        {
            trackedSubjects.Remove(p);
            p.OnRemovedLight(this);
            GD.Print($"GlowingCoral removed light from {body.Name}");
        }
    }

    public override void _ExitTree()
    {
        foreach (var p in trackedSubjects)
            p.OnRemovedLight(this);
        trackedSubjects.Clear();
    }
}
