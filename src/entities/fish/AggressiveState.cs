using Godot;

namespace Yotf;

public class AggressiveState : IFishState
{
    public FishState Type => FishState.Aggressive;
    public bool IsPhotographable => true;

    private float attackCooldown;

    public void Enter(Fish fish)
    {
        attackCooldown = 0f;
    }

    public void Exit(Fish fish) { }

    public void Update(Fish fish, float delta)
    {
        if (fish.ThreatTarget == null)
        {
            fish.SetState(fish.WanderingState);
            return;
        }

        float dist = fish.GlobalPosition.DistanceTo(fish.ThreatTarget.GlobalPosition);

        if (dist > fish.Profile.AggroLeashRadius)
        {
            fish.SetState(fish.WanderingState);
            return;
        }

        fish.SmoothMoveTo(fish.ThreatTarget.GlobalPosition, fish.Profile.FleeSpeed, delta);

        attackCooldown -= delta;
        if (dist <= fish.Profile.AttackRange && attackCooldown <= 0f)
        {
            //fish.EmitSignal(Fish.SignalName.Attacked, fish.ThreatTarget);
            fish.Attacked?.Invoke(fish.ThreatTarget);
            attackCooldown = fish.Profile.AttackCooldown;
        }
    }

    public void OnThreatDetected(Fish fish, Node3D threat) { }

    public void OnThreatLost(Fish fish)
    {
        fish.SetState(fish.WanderingState);
    }

    public void OnNoiseHeard(Fish fish, float level, Vector3 source) { }
}
