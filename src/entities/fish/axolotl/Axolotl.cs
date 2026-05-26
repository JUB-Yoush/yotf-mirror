using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Axolotl : Fish
{
    public override void _Notification(int what) => this.Notify(what);

    public readonly Wander wanderState = new();

    [Export]
    float bubbleShotSpeed = 20f;

    [Export]
    float bubbleRiseSpeed = 2f;

    [Export]
    float bubbleDeceleration = 0.05f;

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

    // public override void _PhysicsProcess(double delta)
    // {
    //     base._PhysicsProcess(delta);
    // }

    //axl picks direction and moves in it.
    //axl shoots a bubble after reaching direciton
    //if there is a dropped item on the ground axl will try to put it in a bubble
    //anything can get caught in axl bubbles?
    public class Wander : IFishState
    {
        float wanderTimer = 0f;
        float maxWanderTime = 3f;
        Vector3 wanderTarget = Vector3.Zero;

        public void Enter(Fish fish)
        {
            wanderTarget = PickWanderDirection((Axolotl)fish);
        }

        public Vector3 PickWanderDirection(Axolotl axlotl)
        {
            // pick a direction and move to it.
            Vector3 direction = new Vector3(
                GD.Randf() * 2f - 1f,
                (GD.Randf() * 2f - 1f) * 0.3f,
                GD.Randf() * 2f - 1f
            ).Normalized();

            // check for collisions
            //axlotl.Velocity = Vector3.Zero;
            //var targetRay = axlotl.DirectionRay;
            //var targetMesh = fish.GetNode<MeshInstance3D>("TargetMesh");
            //targetMesh.TopLevel = true;
            // targetRay.TopLevel = true;
            // targetRay.GlobalPosition = axlotl.GlobalPosition;
            // targetRay.TargetPosition = direction * axlotl.Profile.WanderRadius;

            var target = direction * axlotl.Profile.WanderRadius;

            // targetRay.ForceRaycastUpdate();
            // if (targetRay.IsColliding())
            // {
            //     GD.Print("Collision");
            //     target = targetRay.GetCollisionPoint();
            // }

            // targetMesh.GlobalPosition = direction;
            //Log.PrintLn(target);
            return target;
        }

        public void Update(Fish fish, float delta)
        {
            var axolotl = (Axolotl)fish;
            wanderTimer += delta;
            if (wanderTimer >= maxWanderTime)
            {
                wanderTimer = -100;
                wanderTarget = PickWanderDirection(axolotl);
                MakeBubble(axolotl);
            }
            //axolotl.SmoothMoveTo(wanderTarget, axolotl.Profile.MoveSpeed, delta);

            //Vector3 toTarget = wanderTarget - axolotl.GlobalPosition;
            // if (toTarget.LengthSquared() < 0.1)
            // {
            //     toTarget = Vector3.Zero;
            // }

            //Vector3 dir = toTarget.Normalized();
            //Log.PrintLn(toTarget, dir, axolotl.Velocity);
            //axolotl.SmoothMoveTo(wanderTarget, axolotl.Profile.MoveSpeed, delta);
        }

        public void MakeBubble(Axolotl axolotl)
        {
            var dir = axolotl.GlobalBasis.Z;
            var bubble = Bubble.New(
                dir,
                axolotl.bubbleShotSpeed,
                axolotl.bubbleRiseSpeed,
                axolotl.bubbleDeceleration
            );
            axolotl.AddChild(bubble);
        }
    }
}
