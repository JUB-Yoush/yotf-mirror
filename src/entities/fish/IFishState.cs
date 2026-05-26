using Godot;

namespace Yotf;

public interface IFishState
{
    public bool IsPhotographable
    {
        get => true;
    }

    void Update(Fish fish, float delta);

    virtual void Enter(Fish fish) { }
    virtual void Exit(Fish fish) { }

    // sensory events
    virtual void OnThreatDetected(Fish fish, Node3D threat) { }
    virtual void OnThreatLost(Fish fish) { }
    virtual void OnNoiseHeard(Fish fish, float level, Vector3 source) { }
    virtual void OnRadiusEntered(Fish fish, float level, Vector3 source) { }
}
