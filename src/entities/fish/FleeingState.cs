using Godot;

public class FleeingState : IFishState
{
    public FishState Type => FishState.Fleeing;
    public bool IsPhotographable => true;

    private float _fleeTimer;

    public void Enter(Fish fish)
    {
        _fleeTimer = 0f;
    }

    public void Exit(Fish fish) { }

    public void Update(Fish fish, float delta)
    {
        if (fish.ThreatTarget != null)
            fish.ThreatPosition = fish.ThreatTarget.GlobalPosition;

        Vector3 awayDir = (fish.GlobalPosition - fish.ThreatPosition).Normalized();
        Vector3 fleeTarget = fish.GlobalPosition + awayDir * fish.Profile.FleeDistance;

        bool arrived = fish.SmoothMoveTo(fleeTarget, fish.Profile.FleeSpeed, delta);
        _fleeTimer += delta;

        if (arrived || _fleeTimer >= fish.Profile.FleeTimeout)
            fish.SetState(fish.WanderingState);
    }

    public void OnThreatDetected(Fish fish, Node3D threat)
    {
        // Reset the timer so a new nearby threat keeps us fleeing.
        fish.ThreatPosition = threat.GlobalPosition;
        _fleeTimer = 0f;
    }

    public void OnThreatLost(Fish fish)
    {
        fish.SetState(fish.WanderingState);
    }

    public void OnNoiseHeard(Fish fish, float level, Vector3 source)
    {
        fish.ThreatPosition = source;
        _fleeTimer = 0f;
    }
}
