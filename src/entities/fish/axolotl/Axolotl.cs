using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Axolotl : Fish, IBubbleable, IHearNoise
{
    public override void _Notification(int what) => this.Notify(what);

    private readonly StateMachine<State> stateMachine = new();

    enum State
    {
        Wander,
        Chase,
        Flee,
    }

    [Export]
    float bubbleShotSpeed = 20f;

    [Export]
    float bubbleRiseSpeed = 2f;

    [Export]
    float bubbleDeceleration = 0.05f;

    [Export]
    float minimumWanderRange = 3f;

    [Node]
    public required MeshInstance3D NavBox { set; get; }

    [Node]
    public required MeshInstance3D NavBox2 { set; get; }

    //wandering
    float wanderTimer = 0f;
    float maxWanderTime = 10f;
    Vector3 wanderTarget = Vector3.Zero;
    NavNode? CurrentNode
    {
        set
        {
            field = value;
            if (field != null)
            {
                NavBox.GlobalPosition = field!.GlobalPosition;
            }
        }
        get;
    } = null;
    NavGraph navGraph = null!;

    // chasing
    readonly List<IBubbleable> bubbleTargets = [];
    Vector3 bubbleTargetPosition = Vector3.Zero;
    Vector3 lastKnownLocation = Vector3.Zero;
    float lastKnownSeekTimer = 5f;
    float maxLastKnownSeekTimer = 5f;
    float endDistance = 1f;
    bool atBubbleTarget;
    Tween? tween = null;

    //fleeing
    private float fleeTimer;

    public bool AxolotlTargets
    {
        get => false;
    }

    #region IBubbleable
    public Bubble? BubbleJail { get; set; }
    Mesh IBubbleable.Mesh => Mesh.Mesh;

    public void PutInBubble()
    {
        Mesh.Visible = false;
        DetectionZone.Monitoring = false;
    }

    public void FreeFromBubble()
    {
        Mesh.Visible = true;
        DetectionZone.Monitoring = true;
    }
    #endregion IBubbleable

    /*
     * axolotl wanders randomly until bubbleable thing (that isn't already in bubble) is found in it's detection range
     * it then goes up to that thing and bubbles it.
     * If you "press" the axolotl you can make it shoot a bubble
    */
    public override void _Ready()
    {
        navGraph = this.SceneRoot().GetNode<NavGraph>()!;
        CurrentRoom = AssignCurrentRoom();
        stateMachine.AddState(State.Wander, WanderUpdate, WanderEnter);
        stateMachine.AddState(State.Flee, FleeUpdate);
        stateMachine.AddState(State.Chase, ChaseUpdate);
        stateMachine.State = State.Wander;

        DetectionZone.BodyEntered += OnDetectionBodyEntered;
        DetectionZone.BodyExited += OnDetectionBodyExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        stateMachine.Update(delta);
        MoveAndSlide();
    }

    private void OnDetectionBodyExited(Node3D body)
    {
        if (
            body is IBubbleable bubbleable
            && bubbleable.AxolotlTargets
            && stateMachine.State != State.Flee
        )
        {
            bubbleTargets.Remove(bubbleable);
            if (bubbleTargets.Count == 0)
            {
                //    SetState(wanderState);
            }
        }
    }

    private void OnDetectionBodyEntered(Node3D body)
    {
        if (body is IBubbleable bubbleable && bubbleable.AxolotlTargets)
        {
            bubbleTargets.Add(bubbleable);
            //stateMachine.State = State.Chase;
        }
    }

    internal bool SmoothMoveTo(Vector3 target, float speed, float arrivalThreshold, float delta)
    {
        target -= GlobalPosition;
        Vector3 dir = target.Normalized();

        Velocity = MiscExt.V3Lerp(Velocity, target.Normalized() * speed, RotationSpeed * delta);

        float targetYaw = Mathf.Atan2(dir.X, dir.Z);
        GlobalRotation = GlobalRotation with
        {
            Y = Mathf.LerpAngle(GlobalRotation.Y, targetYaw, RotationSpeed * delta),
        };

        Log.PrintLn(target.LengthSquared(), arrivalThreshold);
        return target.LengthSquared() < arrivalThreshold;
    }

    public void MakeBubble(Vector3 dir)
    {
        dir = dir.Normalized();
        var bubble = Bubble.New(this, dir, bubbleShotSpeed, bubbleRiseSpeed, bubbleDeceleration);
        AddChild(bubble);
    }

    public void OnNoiseHeard(Node3D noiseNode, float dB, SFX noise)
    {
        //TODO (j) check if noise is threatening
        ThreatTarget = noiseNode;
        stateMachine.State = State.Flee;
    }

    public void WanderEnter()
    {
        CurrentNode = navGraph.NodeClosestTo(GlobalPosition);
    }

    public void WanderUpdate(float delta)
    {
        // TODO(j) BubbleJail should be a state
        // if (BubbleJail != null)
        // {
        //     GlobalPosition = BubbleJail.GlobalPosition;
        //     Velocity = Vector3.Zero;
        //     return;
        // }

        if (SmoothMoveTo(CurrentNode!.GlobalPosition, MoveSpeed, ArrivalThreshold, delta))
        {
            CurrentNode = PickWanderTarget();
        }
    }

    //NavAgent.GetNextPathPosition();

    public bool AtCurrentTarget(float arrivalThreshold = 0.2f)
    {
        return (GlobalPosition.LengthSquared() - CurrentNode!.GlobalPosition.LengthSquared())
            < arrivalThreshold;
    }

    public NavNode PickWanderTarget()
    {
        var next = CurrentNode!.neighbors[GD.RandRange(0, CurrentNode.neighbors.Length - 1)];
        CreateTween()
            .TweenFn<Vector3>(
                (target) => LookAt(target),
                GlobalRotation,
                next.GlobalPosition.Normalized(),
                0.1f
            );
        return next;
    }

    private bool IsWithinRange(Vector3 moveTarget) =>
        (moveTarget - GlobalPosition - CurrentRoom!.GlobalPosition).Length() <= WanderRadius;

    public async void ChaseUpdate(float delta)
    {
        var currentTarget = bubbleTargets[0];
        if (atBubbleTarget)
        {
            if (tween != null)
                return;

            tween = CreateTween();

            tween.TweenFn<Vector3>(
                (velocity) => Velocity = velocity,
                Velocity,
                Vector3.Zero,
                2f,
                true
            );
            tween.TweenFn<Vector3>(
                (target) => LookAt(target),
                GlobalPosition,
                currentTarget.Spatial.GlobalPosition,
                3f,
                true
            );
            tween.Fn(() =>
            {
                MakeBubble(currentTarget.Spatial.Position - Position);
                bubbleTargets.Pop(0);
                if (bubbleTargets.Count == 0)
                {
                    stateMachine.State = State.Wander;
                }
            });
            await tween.Done();
            tween = null;
        }
        else
        {
            atBubbleTarget = SmoothMoveTo(
                currentTarget.GlobalPosition,
                MoveSpeed,
                ArrivalThreshold,
                delta
            );
        }
    }

    public void FleeUpdate(float delta)
    {
        if (ThreatTarget != null)
            ThreatPosition = ThreatTarget.GlobalPosition;

        Vector3 awayDir = (GlobalPosition - ThreatPosition).Normalized();
        Vector3 fleeTarget = GlobalPosition + awayDir * FleeDistance;

        bool arrived = SmoothMoveTo(fleeTarget, FleeSpeed, ArrivalThreshold, delta);
        fleeTimer += delta;

        if (arrived || fleeTimer >= FleeTimeout)
        {
            stateMachine.State = State.Wander;
        }
    }
}
