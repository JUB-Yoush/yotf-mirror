using Godot;

public interface IFishState
{
    FishState Type { get; }

    bool IsPhotographable { get; }

    void Enter(Fish fish);
    void Exit(Fish fish);
    void Update(Fish fish, float delta);

    // sensory events
    void OnThreatDetected(Fish fish, Node3D threat);
    void OnThreatLost(Fish fish);
    void OnNoiseHeard(Fish fish, float level, Vector3 source);
}
