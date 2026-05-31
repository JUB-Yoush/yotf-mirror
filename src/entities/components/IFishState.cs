using Godot;

namespace Yotf;

public interface IFishState<T>
    where T : Fish
{
    public bool IsPhotographable
    {
        get => true;
    }

    void Update(T fish, float delta);

    virtual void Enter(T fish) { }
    virtual void Exit(T fish) { }

    // sensory events
    virtual void OnThreatDetected(T fish, Node3D threat) { }
    virtual void OnThreatLost(T fish) { }
    virtual void OnNoiseHeard(T fish, float level, Vector3 source) { }
    virtual void OnRadiusEntered(T fish, float level, Vector3 source) { }
}
