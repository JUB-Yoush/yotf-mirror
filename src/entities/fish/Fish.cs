using Godot;

namespace Yotf;

public partial class Fish : AnimatableBody3D, IPhotographable
{
    // ====================== SIGNALS ======================

    // emitted when an aggressive fish enters melee range of target
    [Signal]
    public delegate void AttackedEventHandler(Node3D target);

    // emitted on transition into/out of HiddenState so the scene can toggle mesh visibility and collision
    [Signal]
    public delegate void BecameHiddenEventHandler();

    [Signal]
    public delegate void BecameVisibleEventHandler();

    // ====================== REFERENCES ======================

    [ExportCategory("References")]
    public VisibleOnScreenNotifier3D VisibilityNotif = null!;

    [Export]
    public PathFollow3D? SplineFollower;

    // ====================== BEHAVIOUR ======================

    [ExportCategory("Behaviour")]
    [Export]
    public FishProfile Profile = null!;

    // ====================== STATE MACHINE ======================

    public IFishState CurrentState { get; private set; } = null!;
    public FishState State => CurrentState.Type;

    public readonly WanderingState WanderingState = new();
    public readonly FleeingState FleeingState = new();
    public readonly AggressiveState AggressiveState = new();
    public readonly HiddenState HiddenState = new();

    // last known position of a detected threat so FleeingState can continue fleeing after the threat leaves the detection area
    public Vector3 ThreatPosition { get; internal set; }

    // null when no player is in range
    public Node3D? ThreatTarget { get; private set; }

    // ====================== LIFECYCLE ======================

    public override void _Ready()
    {
        VisibilityNotif = GetNode<VisibleOnScreenNotifier3D>("VisibleOnScreenNotifier3D");

        Area3D detectionZone = GetNode<Area3D>("DetectionZone");
        detectionZone.BodyEntered += OnBodyEnterRange;
        detectionZone.BodyExited += OnBodyExitRange;

        IFishState initial = Profile.StartsHidden ? HiddenState : WanderingState;
        CurrentState = initial;
        CurrentState.Enter(this);
    }

    public override void _PhysicsProcess(double delta)
    {
        CurrentState.Update(this, (float)delta);
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

    internal bool SmoothMoveTo(
        Vector3 target,
        float speed,
        float delta,
        float arrivalThreshold = 0.3f
    )
    {
        Vector3 toTarget = target - GlobalPosition;
        if (toTarget.Length() < arrivalThreshold)
            return true;

        Vector3 dir = toTarget.Normalized();
        GlobalPosition += dir * speed * delta;

        float targetYaw = Mathf.Atan2(dir.X, dir.Z);
        Rotation = Rotation with
        {
            Y = Mathf.LerpAngle(Rotation.Y, targetYaw, Profile.RotationSpeed * delta),
        };

        return false;
    }

    // ====================== IPHOTOGRAPHABLE ======================

    // hidden fish are never photographable regardless of screen visibility
    public bool IsInPhoto()
    {
        return CurrentState.IsPhotographable && VisibilityNotif.IsOnScreen();
    }

    public Node3D GetSubject() => this;
}
