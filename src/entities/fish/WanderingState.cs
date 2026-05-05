using Godot;

namespace Yotf;

public class WanderingState : IFishState
{
    public FishState Type => FishState.Wandering;
    public bool IsPhotographable => true;

    private Vector3 wanderTarget;

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
    }

    private void FollowSpline(Fish fish, float delta)
    {
        fish.SplineFollower.Progress += fish.Profile.MoveSpeed * delta;
        fish.SmoothMoveTo(fish.SplineFollower.GlobalPosition, fish.Profile.MoveSpeed, delta);
    }

    private void WanderRandomly(Fish fish, float delta)
    {
        bool arrived = fish.SmoothMoveTo(wanderTarget, fish.Profile.MoveSpeed, delta);
        if (arrived)
            wanderTarget = PickNewTarget(fish);
    }

    private static Vector3 PickNewTarget(Fish fish)
    {
        Vector3 offset =
            new Vector3(
                GD.Randf() * 2f - 1f,
                (GD.Randf() * 2f - 1f) * 0.3f,
                GD.Randf() * 2f - 1f
            ).Normalized() * fish.Profile.WanderRadius;
        return fish.GlobalPosition + offset;
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
