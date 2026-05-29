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

    public IFishState<Axolotl> CurrentState { get; private set; } = null!;

    //wandering
    float wanderTimer = 0f;
    float maxWanderTime = 3f;
    Vector3 wanderTarget = Vector3.Zero;

    // chasing
    List<IBubbleable> bubbleTargets = [];
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
        base._Ready();
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
            stateMachine.State = State.Chase;
        }
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

        Velocity = MiscExt.V3Lerp(Velocity, target * speed, Profile.RotationSpeed * delta);
        //Velocity = dir * speed;

        float targetYaw = Mathf.Atan2(dir.X, dir.Z);
        GlobalRotation = GlobalRotation with
        {
            Y = Mathf.LerpAngle(GlobalRotation.Y, targetYaw, Profile.RotationSpeed * delta),
        };

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
        Log.PrintLn("wander");
        wanderTarget = PickWanderDirection();
        NavAgent.TargetPosition = wanderTarget;
    }

    public void WanderUpdate(float delta)
    {
        if (BubbleJail != null)
        {
            GlobalPosition = BubbleJail.GlobalPosition;
            Velocity = Vector3.Zero;
            return;
        }
        wanderTimer += (float)delta;
        if (wanderTimer >= maxWanderTime)
        {
            wanderTimer = 0;
            wanderTarget = PickWanderDirection();
            NavAgent.TargetPosition = wanderTarget;
            MakeBubble(GlobalBasis.Z);
        }
        // TODO(j) ignore the Y of next path position as I think it is always level to the floor. movement is all wack uhahsdfasdf
        //SmoothMoveTo(NavAgent.GetNextPathPosition(), Profile.MoveSpeed, (float)delta);
        SmoothMoveTo(wanderTarget, Profile.MoveSpeed, (float)delta);
    }

    public Vector3 PickWanderDirection()
    {
        // pick a direction and move to it.
        var stillPickingDir = true;
        var target = new Vector3();
        while (stillPickingDir)
        {
            Vector3 direction = new Vector3(
                GD.Randf() * 2f - 1f,
                (GD.Randf() * 2f - 1f) * 1f,
                GD.Randf() * 2f - 1f
            ).Normalized();

            var targetRay = DirectionRay;
            targetRay.TopLevel = true;
            targetRay.GlobalPosition = GlobalPosition;
            targetRay.TargetPosition = direction * minimumWanderRange;

            target = direction * Profile.WanderRadius;
            targetRay.ForceRaycastUpdate();
            stillPickingDir = targetRay.IsColliding();

            // stillPickingDir =
            //     (target - axlotl.GlobalPosition).Length() < axlotl.minimumWanderRange;
        }

        // check for collisions

        // targetMesh.GlobalPosition = direction;
        //Log.PrintLn(target);
        return target;
    }

    public async void ChaseUpdate(float delta)
    {
        Log.PrintLn("chase");
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
                Profile.MoveSpeed,
                delta,
                5f
            );
        }
    }

    public void FleeUpdate(float delta)
    {
        Log.PrintLn("flee");
        if (ThreatTarget != null)
            ThreatPosition = ThreatTarget.GlobalPosition;

        Vector3 awayDir = (GlobalPosition - ThreatPosition).Normalized();
        Vector3 fleeTarget = GlobalPosition + awayDir * Profile.FleeDistance;

        bool arrived = SmoothMoveTo(fleeTarget, Profile.FleeSpeed, delta);
        fleeTimer += delta;

        if (arrived || fleeTimer >= Profile.FleeTimeout)
        {
            stateMachine.State = State.Wander;
        }
    }
}
