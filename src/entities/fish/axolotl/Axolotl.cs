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
    Vector3 wanderTarget = Vector3.Zero;

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

    public void MakeBubble(Vector3 dir)
    {
        dir = dir.Normalized();
        var bubble = Bubble.New(this, dir, bubbleShotSpeed, bubbleRiseSpeed, bubbleDeceleration);
        AddChild(bubble);
        InAction = true;
        CreateTween().Fn(() => InAction = false, 2);
    }

    public void OnNoiseHeard(Node3D noiseNode, float dB, SFX noise)
    {
        ThreatTarget = noiseNode;
        //noiseNode.TreeExited =>
        stateMachine.State = State.Flee;

        var fleeDir = (GlobalPosition - ThreatTarget!.GlobalPosition).Normalized();
        CreateTween()
            .TweenFn<Vector3>(
                (target) => LookAt(GlobalPosition - target),
                -GlobalTransform.Basis.Z,
                fleeDir,
                1
            );
    }

    public void BubbleUpdate(float delta)
    {
        if (BubbleJail != null)
        {
            GlobalPosition = BubbleJail!.GlobalPosition;
            Velocity = Vector3.Zero;
        }
    }

    public void WanderEnter()
    {
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

            tween.TweenFn<Vector3>(
                (velocity) => Velocity = velocity,
                Velocity,
                Vector3.Zero,
                2f,
                true
            );
            tween.TweenFn<Vector3>(
                (target) => LookAt(target),
                GlobalTransform.Basis.Z,
                bubbleDir,
                1f,
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
            tween.Finished += () => tween = null;
        }
    }

    public void FleeUpdate(float delta)
    {
        if (ThreatTarget != null)
            ThreatPosition = ThreatTarget.GlobalPosition;

        Vector3 awayDir = (GlobalPosition - ThreatPosition).Normalized();
        Vector3 fleeTarget = GlobalPosition + awayDir * FleeDistance;

        bool arrived = SmoothMoveTo(fleeTarget, FleeSpeed, delta, ArrivalThreshold);
        fleeTimer += delta;

        if (arrived || fleeTimer >= FleeTimeout)
        {
            stateMachine.State = State.Wander;
        }
    }
}
