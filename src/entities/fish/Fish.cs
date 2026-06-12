using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Yotf;

/// <summary>
/// Base Fish Class
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class Fish : CharacterBody3D, IPhotographable, IOnMiniMap, ISonarable, ITakeDamage
{
    public override void _Notification(int what) => this.Notify(what);

    public enum Size
    {
        Small,
        Large,
    }

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

    [Node]
    public required Node3D RayCastContainer { set; get; }

    [Export]
    public bool AIIsOn = true;

    [Export]
    public PathFollow3D? SplineFollower;

    [Export]
    public FishRoom? CurrentRoom { get; set; }

    [Export]
    public Lab LabLayer = null!;

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

    [Export]
    public bool getsBaited = false;

    [Export]
    public float MaxHp = 10f;

    public float Hp = 10f;

    [Export]
    public Size size = Fish.Size.Small;

    // last known position of a detected threat so FleeingState can continue fleeing after the threat leaves the detection area
    public Vec3 ThreatPosition { get; internal set; }

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
    public bool Discovered { get; set; }
    public float Health { get; set; }
    public bool Invincible { get; set; }

    public List<IGiveLight> NearbyLights { get; set; } = [];

    public bool IsDead { get; set; }

    public FishRoom AssignCurrentRoom()
    {
        FishRoom currentClosest = null!;
        foreach (
            var room in this.SceneRoot()
                .GetNode($"RoomMarkers{LabLayer.Index}")
                .GetNodes<FishRoom>()
        )
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

    internal bool SmoothMoveTo(Vec3 target, float speed, float delta, float arrivalThreshold = 0.1f)
    {
        target -= GlobalPosition;
        Vec3 dir = target.Normalized();

        Velocity = Velocity.Lerp(target.Normalized() * speed, WanderRotationSpeed * delta);

        float targetYaw = Mathf.Atan2(dir.X, dir.Z);
        GlobalRotation = GlobalRotation with
        {
            Y = Mathf.LerpAngle(GlobalRotation.Y, targetYaw, WanderRotationSpeed * delta),
        };
        return target.LengthSquared() < arrivalThreshold;
    }

    public NavNode PickWanderTarget(bool sameRoom = false, bool turnTowards = false)
    {
        NavNode next = CurrentNode!.RandomNeighbor();
        while (next.Room != CurrentRoom && sameRoom)
        {
            next = CurrentNode.RandomNeighbor();
        }

        if (turnTowards)
        {
            CreateTween()
                .TweenFn<Vec3>(
                    (target) => LookAt(target),
                    GlobalRotation,
                    next.GlobalPosition.Normalized(),
                    0.1f
                );
        }
        return next;
    }

    public bool IsInPhoto()
    {
        if (!VisibilityNotif.IsOnScreen())
        {
            return false;
        }

        var player = this.SceneRoot().GetNode<Player>()!;
        var playerSeeArea = player.GetNode<Area3D>("PlayerCanSeeIt");
        var playerCam = this.SceneRoot()
            .GetNode<Player>()
            .GetNode<CameraManager>()
            .GetNode<Camera3D>()!;
        foreach (var ray in RayCastContainer.GetChildren().Cast<RayCast3D>())
        {
            ray.GlobalPosition = GlobalPosition;
            ray.TargetPosition = (playerCam.GlobalPosition - GlobalPosition) * 2f;
            ray.ForceRaycastUpdate();
            if (ray.IsColliding())
            {
                if (ray.GetCollider() is Area3D)
                {
                    return true;
                }
                else { }
            }
        }
        return false;
        //return VisibilityNotif.IsOnScreen() && col;
    }

    public void TakeDamage(float amount, Vec3 knockback, Node3D source) { }

    public override void _Ready()
    {
        Debug.Assert(LabLayer != null, $"Fish {Name} created without assigning Layer");

        foreach (var ray in RayCastContainer.GetChildren().Cast<RayCast3D>())
        {
            ray.TopLevel = true;
            ray.Rotation = Vector3.Zero;
            ray.CollideWithAreas = true;
            ray.SetCollisionMaskValue(1, true);
            ray.SetCollisionMaskValue(2, true);
        }

        Lab.CurrentLabUpdated += OnLabUpdated;
        navGraph = this.SceneRoot().GetNode<NavGraph>($"NavGraph{LabLayer.Index}")!;
        // Debug.Assert(
        //     navGraph != null,
        //     "Navgraph is null, fish probably init'ed first or there is no nav graph"
        // );
    }

    private void OnLabUpdated(Lab lab)
    {
        if (lab == LabLayer)
        {
            Log.PrintLn("my time");
        }
        // if (lab.Index == Layer)
        // {
        //     ProcessMode = ProcessModeEnum.Pausable;
        // }
        // else
        // {
        //     ProcessMode = ProcessModeEnum.Disabled;
        //     Visible = false;
        // }
    }

    public virtual void FoundBait(Bait bait) { }

    public override void _ExitTree()
    {
        Lab.CurrentLabUpdated -= OnLabUpdated;
    }
}
