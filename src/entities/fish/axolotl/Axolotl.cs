using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Axolotl : Fish, IBubbleable, IHearNoise, IDoesAction
{
    public override void _Notification(int what) => this.Notify(what);

    private readonly StateMachine<State> stateMachine = new();

    enum State
    {
        Wander,
        Chase,
        Flee,
        Bubbled,
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
    public required MeshInstance3D NavBox2 { set; get; }

    //wandering
    float wanderTimer = 0f;
    float maxWanderTime = 10f;
    Vec3 wanderTarget = Vec3.Zero;

    // chasing
    readonly List<IBubbleable> bubbleTargets = [];
    Vec3 bubbleTargetPosition = Vec3.Zero;
    Vec3 lastKnownLocation = Vec3.Zero;
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

    public Bubble? BubbleJail { get; set; }
    Mesh IBubbleable.Mesh => Mesh.Mesh;

    public bool InAction { get; set; }

    public void PutInBubble()
    {
        stateMachine.State = State.Bubbled;
        Mesh.Visible = false;
        DetectionZone.Monitoring = false;
    }

    public void FreeFromBubble()
    {
        stateMachine.State = State.Wander;
        Mesh.Visible = true;
        DetectionZone.Monitoring = true;
    }

    public override void _Ready()
    {
        navGraph = this.SceneRoot().GetNode<NavGraph>()!;
        CurrentRoom ??= AssignCurrentRoom();

        stateMachine.AddState(State.Wander, WanderUpdate, WanderEnter);
        stateMachine.AddState(State.Flee, FleeUpdate);
        stateMachine.AddState(State.Chase, ChaseUpdate);
        stateMachine.AddState(State.Bubbled, BubbleUpdate);

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
            && bubbleable.BubbleJail == null
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
            stateMachine.State = State.Chase;
        }
    }

    public void MakeBubble(Vec3 dir)
    {
        dir = dir.Normalized();
        var bubble = Bubble.New(this, dir, bubbleShotSpeed, bubbleRiseSpeed, bubbleDeceleration);
        AddChild(bubble);
        InAction = true;
        CreateTween().Fn(() => InAction = false, 2);
    }

    public void OnNoiseHeard(Node3D noiseNode, float dB, string noise)
    {
        ThreatTarget = noiseNode;
        //noiseNode.TreeExited =>
        stateMachine.State = State.Flee;

        var fleeDir = (GlobalPosition - ThreatTarget!.GlobalPosition).Normalized();
        CreateTween()
            .TweenFn<Vec3>(
                (target) => LookAt(GlobalPosition - target),
                -GlobalTransform.Basis.Z,
                fleeDir,
                1
            );
    }

    public void BubbleUpdate(float delta)
    {
        if (GodotObject.IsInstanceValid(BubbleJail))
        {
            GlobalPosition = BubbleJail!.GlobalPosition;
            Velocity = Vec3.Zero;
        }
    }

    public void WanderEnter()
    {
        ThreatTarget = null;
        CurrentNode = navGraph.NodeClosestTo(GlobalPosition);
        CurrentRoom = AssignCurrentRoom();
    }

    public void WanderUpdate(float delta)
    {
        if (SmoothMoveTo(CurrentNode!.GlobalPosition, WanderSpeed, delta, ArrivalThreshold))
        {
            CurrentNode = PickWanderTarget();
        }
    }

    public void ChaseUpdate(float delta)
    {
        if (bubbleTargets.Count == 0)
        {
            stateMachine.State = State.Wander;
            return;
        }
        var currentTarget = bubbleTargets[0];

        if (
            SmoothMoveTo(
                currentTarget.Spatial.GlobalPosition,
                WanderSpeed,
                delta,
                ArrivalThreshold / 2
            )
        )
        {
            var bubbleDir = (GlobalPosition - currentTarget.Spatial.GlobalPosition).Normalized();
            if (tween != null)
                return;

            tween = CreateTween();

            tween.TweenFn<Vec3>((velocity) => Velocity = velocity, Velocity, Vec3.Zero, 2f, true);
            tween.TweenFn<Vec3>(
                (target) => LookAt(target),
                GlobalTransform.Basis.Z,
                bubbleDir,
                1f,
                true
            );
            tween.Fn(() =>
            {
                MakeBubble(currentTarget.Spatial.Position - Position);
                if (bubbleTargets.Count == 0)
                {
                    stateMachine.State = State.Wander;
                }
                else
                {
                    bubbleTargets.Pop(0);
                }
            });
            tween.Finished += () => tween = null;
        }
    }

    public void FleeUpdate(float delta)
    {
        if (GodotObject.IsInstanceValid(ThreatTarget))
            ThreatPosition = ThreatTarget.GlobalPosition;

        Vec3 awayDir = (GlobalPosition - ThreatPosition).Normalized();
        Vec3 fleeTarget = GlobalPosition + awayDir * FleeDistance;

        bool arrived = SmoothMoveTo(fleeTarget, FleeSpeed, delta, ArrivalThreshold);
        fleeTimer += delta;

        if (arrived || fleeTimer >= FleeTimeout)
        {
            stateMachine.State = State.Wander;
        }
    }
}
