using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Axolotl : Fish, IBubbleable
{
    public override void _Notification(int what) => this.Notify(what);

    public readonly Wander wanderState = new();

    [Export]
    float bubbleShotSpeed = 20f;

    [Export]
    float bubbleRiseSpeed = 2f;

    [Export]
    float bubbleDeceleration = 0.05f;

    [Export]
    float minimumWanderRange = 3f;

    IBubbleable? bubbleTarget = null;

    public Bubble? BubbleJail { get; set; }

    Mesh IBubbleable.Mesh => Mesh.Mesh;

    /*
     * axolotl wanders randomly until bubbleable thing (that isn't already in bubble) is found in it's detection range
     * it then goes up to that thing and bubbles it.
     * If you "press" the axolotl you can make it shoot a bubble
    */
    public override void _Ready()
    {
        base._Ready();
        SetState(wanderState);
        DetectionZone.BodyEntered += OnBodyEntered;
        DetectionZone.BodyExited += OnBodyExited;
    }

    private void OnBodyExited(Node3D body) { }

    private void OnBodyEntered(Node3D body)
    {
        if (body is IBubbleable bubbleable)
        {
            bubbleTarget = bubbleable;
        }
    }

    internal bool SmoothMoveTo(
        Vector3 target,
        float speed,
        float delta,
        float arrivalThreshold = 0.1f
    )
    {
        //target = GlobalPosition - target;
        target -= GlobalPosition;
        // if (toTarget.LengthSquared() < arrivalThreshold)
        //     return true;
        Vector3 dir = target.Normalized();

        Velocity = MiscExt.V3Lerp(Velocity, target * speed, Profile.RotationSpeed * delta);
        //Velocity = dir * speed;

        float targetYaw = Mathf.Atan2(dir.X, dir.Z);
        GlobalRotation = GlobalRotation with
        {
            Y = Mathf.LerpAngle(GlobalRotation.Y, targetYaw, Profile.RotationSpeed * delta),
        };

        return false;
    }

    public void MakeBubble()
    {
        var dir = GlobalBasis.Z;
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

    public class Wander : IFishState
    {
        Axolotl axolotl = null!;
        float wanderTimer = 0f;
        float maxWanderTime = 3f;
        Vector3 wanderTarget = Vector3.Zero;

        public void Enter(Fish fish)
        {
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

        public void Update(Fish fish, float delta)
        {
            var axolotl = (Axolotl)fish;
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
                axolotl.MakeBubble();
            }
            //wanderTarget = axolotl.SceneRoot().GetNode<PlayerController>()!.GlobalPosition;
            axolotl.SmoothMoveTo(
                axolotl.NavAgent.GetNextPathPosition(),
                axolotl.Profile.MoveSpeed,
                delta
            );
            Log.PrintLn(
                wanderTarget,
                axolotl.NavAgent.GetNextPathPosition(),
                axolotl.Velocity,
                wanderTarget == axolotl.Position
            );
            //axolotl.SmoothMoveTo(wanderTarget, axolotl.Profile.MoveSpeed, delta);
        }
    }

    public class Chasing : IFishState
    {
        Vector3 targetPosition = Vector3.Zero;
        Axolotl axolotl = null!;

        public void Update(Fish fish, float delta)
        {
            axolotl ??= (Axolotl)fish;
            axolotl.SmoothMoveTo(targetPosition, axolotl.Profile.MoveSpeed, delta);
        }
    }
}
