using Godot;

namespace Yotf;

public class FleeingState : IFishState
{
    public bool IsPhotographable => true;

    private float fleeTimer;

    public void Enter(Fish fish)
    {
        fleeTimer = 0f;
    }

    public void Exit(Fish fish) { }

    public void Update(Fish fish, float delta)
    {
        if (fish.ThreatTarget != null)
            fish.ThreatPosition = fish.ThreatTarget.GlobalPosition;

        Vector3 awayDir = (fish.GlobalPosition - fish.ThreatPosition).Normalized();
        Vector3 fleeTarget = fish.GlobalPosition + awayDir * fish.Profile.FleeDistance;

        //bool arrived = fish.SmoothMoveTo(fleeTarget, fish.Profile.FleeSpeed, delta);
        bool arrived = true;
        fleeTimer += delta;

        if (arrived || fleeTimer >= fish.Profile.FleeTimeout)
            fish.SetState(fish.WanderingState);
    }

    public void OnThreatDetected(Fish fish, Node3D threat)
    {
        // reset the timer so a new nearby threat keeps us fleeing
        fish.ThreatPosition = threat.GlobalPosition;
        fleeTimer = 0f;
    }

    public void OnThreatLost(Fish fish)
    {
        fish.SetState(fish.WanderingState);
    }

    public void OnNoiseHeard(Fish fish, float level, Vector3 source)
    {
        fish.ThreatPosition = source;
        fleeTimer = 0f;
    }
}
