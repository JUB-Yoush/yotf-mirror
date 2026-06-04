using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Fish : CharacterBody3D, IPhotographable, IOnMiniMap
{
    public override void _Notification(int what) => this.Notify(what);

    readonly Routine routine = new();

    // ====================== REFERENCES ======================

    [Node]
    public required VisibleOnScreenNotifier3D VisibilityNotif { set; get; }

    [Node]
    public required RayCast3D DirectionRay { set; get; }

    [Node]
    public required Area3D DetectionZone { set; get; }

    [Node]
    public required MeshInstance3D Mesh { set; get; }

    [Node]
    public required MeshInstance3D NavBox { set; get; }

    [Export]
    public PathFollow3D? SplineFollower;

    [Export]
    public FishRoom? CurrentRoom { get; set; }

    [ExportCategory("FishProfile")]
    [Export]
    public float WanderSpeed = 3f;

    [Export]
    public float WanderRotationSpeed = 4f;

    [Export]
    public float WanderRadius = 6f;

    [Export]
    public float FleeSpeed = 7f;

    [Export]
    public float FleeRotationSpeed = 7f;

    [Export]
    public float FleeDistance = 12f;

    [Export]
    public float FleeTimeout = 6f;

    [Export]
    public float NavRandomOffsetRange = 3f;

    [Export]
    public float ArrivalThreshold = 10f;

    [Export(PropertyHint.Range, "0,1")]
    public float NoiseTolerance = 0.4f;

    // last known position of a detected threat so FleeingState can continue fleeing after the threat leaves the detection area
    public Vector3 ThreatPosition { get; internal set; }

    // null when no player is in range
    public Node3D? ThreatTarget { get; set; }

    internal bool ReturningHome = false;

    internal NavNode? PrevNode;
    internal NavNode? CurrentNode
    {
        set
        {
            PrevNode = field;
            field = value;
            if (field != null)
            {
                NavBox.GlobalPosition = field!.GlobalPosition;
            }
        }
        get;
    } = null;
    internal NavGraph navGraph = null!;

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
    public IPhotographable.PhotoModifier Modifier
    {
        get => IPhotographable.PhotoModifier.None;
        set;
    }

    // public override void _PhysicsProcess(double delta)
    // {
    //     //CurrentState.Update(this, (float)delta);
    //     MoveAndSlide();
    // }

    public FishRoom AssignCurrentRoom()
    {
        FishRoom currentClosest = null!;
        foreach (var room in this.SceneRoot().GetNodes<FishRoom>())
        {
            currentClosest ??= room;
            if (
                (room.GlobalPosition - GlobalPosition).LengthSquared()
                <= (currentClosest.GlobalPosition - GlobalPosition).LengthSquared()
            )
            {
                currentClosest = room;
            }
        }
        return currentClosest;
    }

    internal bool SmoothMoveTo(
        Vector3 target,
        float speed,
        float delta,
        float arrivalThreshold = 0.1f
    )
    {
        target -= GlobalPosition;
        Vector3 dir = target.Normalized();

        Velocity = MiscExt.V3Lerp(
            Velocity,
            target.Normalized() * speed,
            WanderRotationSpeed * delta
        );

        float targetYaw = Mathf.Atan2(dir.X, dir.Z);
        GlobalRotation = GlobalRotation with
        {
            Y = Mathf.LerpAngle(GlobalRotation.Y, targetYaw, WanderRotationSpeed * delta),
        };

        return target.LengthSquared() < arrivalThreshold;
    }

    public NavNode PickWanderTarget(bool sameRoom = true, bool turnTowards = false)
    {
        NavNode next = CurrentNode!.RandomNeighbor();
        while (next.Room != CurrentRoom && sameRoom)
        {
            next = CurrentNode.RandomNeighbor();
        }

        if (turnTowards)
        {
            CreateTween()
                .TweenFn<Vector3>(
                    (target) => LookAt(target),
                    GlobalRotation,
                    next.GlobalPosition.Normalized(),
                    0.1f
                );
        }
        return next;
    }

    // ====================== SENSORY ENTRY POINTS ======================
    private void OnBodyEnterRange(Node3D body)
    {
        // if (body is not CharacterBody3D)
        //     return;
        // ThreatTarget = body;
        // ThreatPosition = body.GlobalPosition;
        // CurrentState.OnThreatDetected(this, body);
    }

    private void OnBodyExitRange(Node3D body)
    {
        // if (body is not CharacterBody3D)
        //     return;
        // if (ThreatTarget == body)
        //     ThreatTarget = null;
        // CurrentState.OnThreatLost(this);
    }

    // // level is 0-1, source is world pos
    // public void OnNoiseHeard(float level, Vector3 source)
    // {
    //     if (level >= Profile.NoiseThreshold)
    //         CurrentState.OnNoiseHeard(this, level, source);
    // }

    // called by whatever gadget reveals hidden fish
    public void Reveal()
    {
        // if (CurrentState is HiddenState)
        //     SetState<Axolotl>(WanderingState);
    }

    public bool IsInPhoto() => VisibilityNotif.IsOnScreen();

    // ====================== INTERNAL HELPERS ======================

    // internal void SetState(IFishState newState)
    // {
    //     // if (CurrentState == newState)
    //     //     return;
    //     // CurrentState.Exit(this);
    //     // CurrentState = newState;
    //     // CurrentState.Enter(this);
    // }

    // ====================== IPHOTOGRAPHABLE ======================

    // hidden fish are never photographable regardless of screen visibility
    // public bool IsInPhoto() => CurrentState.IsPhotographable && VisibilityNotif.IsOnScreen();
    // public bool IsInPhoto() => CurrentState.IsPhotographable && VisibilityNotif.IsOnScreen();
}
