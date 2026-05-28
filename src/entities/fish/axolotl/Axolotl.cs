using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Axolotl : Fish, IBubbleable, IHearNoise
{
    public override void _Notification(int what) => this.Notify(what);

    public readonly Wander wanderState = new();
    public readonly Chasing chaseState = new();
    public readonly Fleeing fleeState = new();

    [Export]
    float bubbleShotSpeed = 20f;

    [Export]
    float bubbleRiseSpeed = 2f;

    [Export]
    float bubbleDeceleration = 0.05f;

    [Export]
    float minimumWanderRange = 3f;

    public IFishState<Axolotl> CurrentState { get; private set; } = null!;

    // chasing
    List<IBubbleable> bubbleTargets = [];
    Vector3 bubbleTargetPosition = Vector3.Zero;
    Vector3 lastKnownLocation = Vector3.Zero;
    float lastKnownSeekTimer = 5f;
    float maxLastKnownSeekTimer = 5f;
    float endDistance = 1f;
    bool atBubbleTarget;

    public Bubble? BubbleJail { get; set; }

    public bool AxolotlTargets
    {
        get => false;
    }

    Mesh IBubbleable.Mesh => Mesh.Mesh;

    /*
     * axolotl wanders randomly until bubbleable thing (that isn't already in bubble) is found in it's detection range
     * it then goes up to that thing and bubbles it.
     * If you "press" the axolotl you can make it shoot a bubble
    */
    public override void _Ready()
    {
        base._Ready();
        //SetState(wanderState);
        DetectionZone.BodyEntered += OnDetectionBodyEntered;
        DetectionZone.BodyExited += OnDetectionBodyExited;
    }

    private void OnDetectionBodyExited(Node3D body)
    {
        if (
            body is IBubbleable bubbleable
            && bubbleable.AxolotlTargets
            && CurrentState != fleeState
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
            //SetState(chaseState);
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

    public void OnNoiseHeard(Node3D noiseNode, float dB, SFX noise)
    {
        Log.PrintLn("im so fucking scared");
        // if in state that lets me be scared{
        ThreatTarget = noiseNode;
        //SetState(fleeState);
    }

    public class Wander : IFishState<Axolotl>
    {
        Axolotl axolotl = null!;
        float wanderTimer = 0f;
        float maxWanderTime = 3f;
        Vector3 wanderTarget = Vector3.Zero;

        public void Enter(Fish fish)
        {
            Log.PrintLn("wander");
            axolotl = (Axolotl)fish;
            wanderTarget = PickWanderDirection((Axolotl)fish);

            //wanderTarget = axolotl.SceneRoot().GetNode<PlayerController>()!.GlobalPosition;
            axolotl.NavAgent.TargetPosition = wanderTarget;
        }

        public static Vector3 PickWanderDirection(Axolotl axlotl)
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

                var targetRay = axlotl.DirectionRay;
                targetRay.TopLevel = true;
                targetRay.GlobalPosition = axlotl.GlobalPosition;
                targetRay.TargetPosition = direction * axlotl.minimumWanderRange;

                target = direction * axlotl.Profile.WanderRadius;
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

        public void Update(Axolotl axolotl, float delta)
        {
            if (axolotl.BubbleJail != null)
            {
                axolotl.GlobalPosition = axolotl.BubbleJail.GlobalPosition;
                axolotl.Velocity = Vector3.Zero;
                return;
            }
            wanderTimer += delta;
            if (wanderTimer >= maxWanderTime)
            {
                wanderTimer = 0;
                wanderTarget = PickWanderDirection(axolotl);
                axolotl.NavAgent.TargetPosition = wanderTarget;
                //wanderTarget = axolotl.SceneRoot().GetNode<PlayerController>()!.GlobalPosition;
                axolotl.MakeBubble(axolotl.GlobalBasis.Z);
            }
            //wanderTarget = axolotl.SceneRoot().GetNode<PlayerController>()!.GlobalPosition;
            axolotl.SmoothMoveTo(
                axolotl.NavAgent.GetNextPathPosition(),
                axolotl.Profile.MoveSpeed,
                delta
            );
            // Log.PrintLn(
            //     wanderTarget,
            //     axolotl.NavAgent.GetNextPathPosition(),
            //     axolotl.Velocity,
            //     wanderTarget == axolotl.Position
            // );
            //axolotl.SmoothMoveTo(wanderTarget, axolotl.Profile.MoveSpeed, delta);
        }
    }

    public class Chasing : IFishState<Axolotl>
    {
        Axolotl axolotl = null!;
        Tween? tween = null;

        public void Enter(Fish fish)
        {
            Log.PrintLn("chase");
            axolotl = (Axolotl)fish;
        }

        public void Exit(Fish fish) { }

        public async void Update(Fish fish, float delta)
        {
            var currentTarget = axolotl.bubbleTargets[0];
            if (axolotl.atBubbleTarget)
            {
                if (tween != null)
                    return;

                // axolotl.Velocity = MiscExt.V3Lerp(axolotl.Velocity, Vector3.Zero, 0.2f);
                // axolotl.MakeBubble(currentTarget.Spatial.Position - axolotl.Position);
                // axolotl.bubbleTargets.Pop(0);
                // if (axolotl.bubbleTargets.Count == 0)
                // {
                //     axolotl.SetState(axolotl.wanderState);
                // }

                tween = axolotl.CreateTween();

                tween.TweenFn<Vector3>(
                    (velocity) => axolotl.Velocity = velocity,
                    axolotl.Velocity,
                    Vector3.Zero,
                    2f,
                    true
                );
                tween.TweenFn<Vector3>(
                    (target) => axolotl.LookAt(target),
                    axolotl.GlobalPosition,
                    currentTarget.Spatial.GlobalPosition,
                    3f,
                    true
                );
                tween.Fn(() =>
                {
                    axolotl.MakeBubble(currentTarget.Spatial.Position - axolotl.Position);
                    axolotl.bubbleTargets.Pop(0);
                    if (axolotl.bubbleTargets.Count == 0)
                    {
                        //axolotl.SetState(axolotl.wanderState);
                    }
                });
                await tween.Done();
                tween = null;
            }
            else
            {
                axolotl.atBubbleTarget = axolotl.SmoothMoveTo(
                    currentTarget.GlobalPosition,
                    axolotl.Profile.MoveSpeed,
                    delta,
                    5f
                );
            }
        }

        public void Update(Axolotl fish, float delta)
        {
            throw new NotImplementedException();
        }
    }

    public class Fleeing : IFishState<Axolotl>
    {
        private float fleeTimer;
        Axolotl axolotl = null!;

        public void Enter(Fish fish)
        {
            Log.PrintLn("gtfo");
            axolotl ??= (Axolotl)fish;
        }

        public void Update(Fish fish, float delta)
        {
            if (axolotl.ThreatTarget != null)
                axolotl.ThreatPosition = axolotl.ThreatTarget.GlobalPosition;

            Vector3 awayDir = (axolotl.GlobalPosition - axolotl.ThreatPosition).Normalized();
            Vector3 fleeTarget = axolotl.GlobalPosition + awayDir * axolotl.Profile.FleeDistance;

            bool arrived = axolotl.SmoothMoveTo(fleeTarget, axolotl.Profile.FleeSpeed, delta);
            fleeTimer += delta;

            if (arrived || fleeTimer >= axolotl.Profile.FleeTimeout)
            {
                //axolotl.SetState(axolotl.wanderState);
            }
        }

        public void OnThreatDetected(Fish fish, Node3D threat)
        {
            // reset the timer so a new nearby threat keeps us fleeing
            axolotl.ThreatPosition = threat.GlobalPosition;
            fleeTimer = 0f;
        }

        public void OnThreatLost(Fish fish)
        {
            //axolotl.SetState(axolotl.wanderState);
        }

        public void Update(Axolotl fish, float delta)
        {
            throw new NotImplementedException();
        }
    }
}
