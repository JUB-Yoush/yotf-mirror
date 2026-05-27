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

    public Bubble? BubbleJail { get; set; }

    Mesh IBubbleable.Mesh => Mesh.Mesh;

    public override void _Ready()
    {
        base._Ready();
        SetState(wanderState);
    }

    internal bool SmoothMoveTo(
        Vector3 target,
        float speed,
        float delta,
        float arrivalThreshold = 0.1f
    )
    {
        // Vector3 toTarget = target - GlobalPosition;
        // if (toTarget.LengthSquared() < arrivalThreshold)
        //     return true;

        Vector3 dir = target.Normalized();
        // Velocity = dir * speed * delta;

        Velocity = target * speed;

        float targetYaw = Mathf.Atan2(dir.X, dir.Z);
        GlobalRotation = GlobalRotation with
        {
            Y = Mathf.LerpAngle(GlobalRotation.Y, targetYaw, Profile.RotationSpeed * delta),
        };

        return false;
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
        float wanderTimer = 0f;
        float maxWanderTime = 3f;
        Vector3 wanderTarget = Vector3.Zero;

        public void Enter(Fish fish)
        {
            wanderTarget = PickWanderDirection((Axolotl)fish);
        }

        public static Vector3 PickWanderDirection(Axolotl axlotl)
        {
            // pick a direction and move to it.
            Vector3 direction = new Vector3(
                GD.Randf() * 2f - 1f,
                (GD.Randf() * 2f - 1f) * 0.3f,
                GD.Randf() * 2f - 1f
            ).Normalized();

            // check for collisions
            var targetRay = axlotl.DirectionRay;
            targetRay.TopLevel = true;
            targetRay.GlobalPosition = axlotl.GlobalPosition;
            targetRay.TargetPosition = direction * axlotl.Profile.WanderRadius;

            var target = direction * axlotl.Profile.WanderRadius;

            // targetRay.ForceRaycastUpdate();
            // if (targetRay.IsColliding())
            // {
            //     Log.PrintLn("Collision");
            //     target = targetRay.GetCollisionPoint();
            // }

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
                MakeBubble(axolotl);
            }
            axolotl.SmoothMoveTo(wanderTarget, axolotl.Profile.MoveSpeed, delta);
        }

        public static void MakeBubble(Axolotl axolotl)
        {
            var dir = axolotl.GlobalBasis.Z;
            var bubble = Bubble.New(
                axolotl,
                dir,
                axolotl.bubbleShotSpeed,
                axolotl.bubbleRiseSpeed,
                axolotl.bubbleDeceleration
            );
            axolotl.AddChild(bubble);
        }
    }
}
