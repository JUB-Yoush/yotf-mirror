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

    [Export]
    public PathFollow3D? SplineFollower;

    [ExportCategory("FishProfile")]
    [Export]
    public float MoveSpeed = 3f;

    [Export]
    public float RotationSpeed = 4f;

    [Export]
    public float WanderRadius = 6f;

    [Export]
    public float FleeSpeed = 7f;

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

    public FishRoom? CurrentRoom { get; set; }

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

    // public override void _PhysicsProcess(double delta)
    // {
    //     //CurrentState.Update(this, (float)delta);
    //     MoveAndSlide();
    // }

    public FishRoom AssignCurrentRoom()
    {
        FishRoom res = null!;
        foreach (var room in this.SceneRoot().GetNodes<FishRoom>())
        {
            res ??= room;
            if (room.GlobalPosition - GlobalPosition <= res.GlobalPosition - GlobalPosition)
            {
                res = room;
            }
        }
        return res;
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

    public bool IsInPhoto() => true;

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
