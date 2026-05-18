using Godot;

namespace Yotf;

public class HiddenState : IFishState
{
    public FishState Type => FishState.Hidden;

    public bool IsPhotographable => false;

    public void Enter(Fish fish)
    {
        //fish.EmitSignal(Fish.SignalName.BecameHidden);
        fish.BecameHidden?.Invoke();
    }

    public void Exit(Fish fish)
    {
        fish.BecameVisible?.Invoke();
    }

    public void Update(Fish fish, float delta) { }

    // ignore all stimuli while hidden
    public void OnThreatDetected(Fish fish, Node3D threat) { }

    public void OnThreatLost(Fish fish) { }

    public void OnNoiseHeard(Fish fish, float level, Vector3 source) { }
}
