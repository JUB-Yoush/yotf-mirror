using Godot;

public interface IFishState
{
    FishState Type { get; }

    // When false, IsInPhoto() returns false regardless of screen visibility.
    // Use this for hidden/cloaked fish that shouldn't be photographable.
    bool IsPhotographable { get; }

    void Enter(Fish fish);
    void Exit(Fish fish);
    void Update(Fish fish, float delta);

    // Sensory events — Fish calls these when the scene detects stimuli.
    // States decide whether and how to react.
    void OnThreatDetected(Fish fish, Node3D threat);
    void OnThreatLost(Fish fish);
    void OnNoiseHeard(Fish fish, float level, Vector3 source);
}
