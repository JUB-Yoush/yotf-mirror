using Godot;

namespace Yotf;

public class WanderingState : IFishState
{
    public FishState Type => FishState.Wandering;
    public bool IsPhotographable => true;

    private Vector3 wanderTarget;
    private float swimTime = 0f;

    public void Enter(Fish fish)
    {
        wanderTarget = PickNewTarget(fish);
    }

    public void Exit(Fish fish) { }

    public void Update(Fish fish, float delta)
    {
        if (fish.SplineFollower != null)
            FollowSpline(fish, delta);
        else
            WanderRandomly(fish, delta);

        Wiggle(fish, delta);
    }

    private void FollowSpline(Fish fish, float delta)
    {
        if (fish.SplineFollower == null)
            return;

        if (fish.SmoothMoveTo(fish.SplineFollower.GlobalPosition, fish.Profile.MoveSpeed, delta))
            fish.SplineFollower.Progress += fish.Profile.MoveSpeed * delta;
    }

    private void WanderRandomly(Fish fish, float delta)
    {
        bool arrived = fish.SmoothMoveTo(wanderTarget, fish.Profile.MoveSpeed, delta);
        if (arrived)
        {
            wanderTarget = PickNewTarget(fish);
        }
    }

    private void Wiggle(Fish fish, float delta)
    {
        swimTime += delta;
        float angle = swimTime * Mathf.Tau;
        fish.Rotation = fish.Rotation with { Z = Mathf.Sin(angle) * 0.5f };
    }

    private static Vector3 PickNewTarget(Fish fish)
    {
        Vector3 direction = new Vector3(
            GD.Randf() * 2f - 1f,
            (GD.Randf() * 2f - 1f) * 0.3f,
            GD.Randf() * 2f - 1f
        ).Normalized();

        // check for collisions
        // fish.Velocity = Vector3.Zero;
        // var targetRay = fish.GetNode<RayCast3D>("TargetRay");
        // var targetMesh = fish.GetNode<MeshInstance3D>("TargetMesh");
        // targetMesh.TopLevel = true;
        // targetRay.TopLevel = true;
        // targetRay.GlobalPosition = fish.GlobalPosition;
        // targetRay.TargetPosition = direction * fish.Profile.WanderRadius;

        var target = direction * fish.Profile.WanderRadius;

        // targetRay.ForceRaycastUpdate();
        // if (targetRay.IsColliding())
        // {
        //     GD.Print("Collision");
        //     target = targetRay.GetCollisionPoint();
        // }

        // targetMesh.GlobalPosition = direction;
        return target;
    }

    public void OnThreatDetected(Fish fish, Node3D threat)
    {
        if (fish.Profile.IsAggressive)
            fish.SetState(fish.AggressiveState);
        else
            fish.SetState(fish.FleeingState);
    }

    public void OnThreatLost(Fish fish) { }

    public void OnNoiseHeard(Fish fish, float level, Vector3 source)
    {
        if (fish.Profile.IsAggressive)
            return;

        fish.ThreatPosition = source;
        fish.SetState(fish.FleeingState);
    }
}
