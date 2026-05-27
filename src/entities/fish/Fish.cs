using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Fish : CharacterBody3D, IPhotographable, IOnMiniMap
{
    public override void _Notification(int what) => this.Notify(what);

    readonly Routine routine = new();

    // ====================== SIGNALS ======================

    // emitted when an aggressive fish enters melee range of target
    public Action<Node3D>? Attacked;
    public Action? BecameHidden;
    public Action? BecameVisible;

    // ====================== REFERENCES ======================

    [Node]
    public required VisibleOnScreenNotifier3D VisibilityNotif { set; get; }

    [Node]
    public required RayCast3D DirectionRay { set; get; }

    [Node]
    public required NavigationAgent3D NavAgent { set; get; }

    [Node]
    public required Area3D DetectionZone { set; get; }

    [Export]
    public PathFollow3D? SplineFollower;

    [Export]
    public MeshInstance3D Mesh;

    // ====================== BEHAVIOUR ======================

    [ExportCategory("Behaviour")]
    [Export]
    public FishProfile Profile = null!;

    // ====================== STATE MACHINE ======================

    public IFishState CurrentState { get; private set; } = null!;

    public readonly WanderingState WanderingState = new();
    public readonly FleeingState FleeingState = new();
    public readonly AggressiveState AggressiveState = new();
    public readonly HiddenState HiddenState = new();

    // last known position of a detected threat so FleeingState can continue fleeing after the threat leaves the detection area
    public Vector3 ThreatPosition { get; internal set; }

    // null when no player is in range
    public Node3D? ThreatTarget { get; private set; }

    public required MeshInstance3D SubjectBoundingMesh
    {
        get => Mesh;
        set;
    }

    public required Node3D Subject
    {
        get => this;
        set;
    }

    // ====================== LIFECYCLE ======================

    public override void _Ready()
    {
        Area3D detectionZone = this.GetNode<Area3D>()!;
        detectionZone.BodyEntered += OnBodyEnterRange;
        detectionZone.BodyExited += OnBodyExitRange;

        IFishState initial = Profile.StartsHidden ? HiddenState : WanderingState;
        CurrentState = initial;
        CurrentState.Enter(this);
    }

    public override void _PhysicsProcess(double delta)
    {
        CurrentState.Update(this, (float)delta);
        MoveAndSlide();
    }

    // ====================== SENSORY ENTRY POINTS ======================
    private void OnBodyEnterRange(Node3D body)
    {
        if (body is not CharacterBody3D)
            return;
        ThreatTarget = body;
        ThreatPosition = body.GlobalPosition;
        CurrentState.OnThreatDetected(this, body);
    }

    private void OnBodyExitRange(Node3D body)
    {
        if (body is not CharacterBody3D)
            return;
        if (ThreatTarget == body)
            ThreatTarget = null;
        CurrentState.OnThreatLost(this);
    }

    // level is 0-1, source is world pos
    public void OnNoiseHeard(float level, Vector3 source)
    {
        if (level >= Profile.NoiseThreshold)
            CurrentState.OnNoiseHeard(this, level, source);
    }

    // called by whatever gadget reveals hidden fish
    public void Reveal()
    {
        if (CurrentState is HiddenState)
            SetState(WanderingState);
    }

    // ====================== INTERNAL HELPERS ======================

    internal void SetState(IFishState newState)
    {
        if (CurrentState == newState)
            return;
        CurrentState.Exit(this);
        CurrentState = newState;
        CurrentState.Enter(this);
    }

    // ====================== IPHOTOGRAPHABLE ======================

    // hidden fish are never photographable regardless of screen visibility
    public bool IsInPhoto() => CurrentState.IsPhotographable && VisibilityNotif.IsOnScreen();
}
