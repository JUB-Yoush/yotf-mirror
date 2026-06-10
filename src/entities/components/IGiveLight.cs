namespace Yotf;

public interface IGiveLight
{
    Light3D LightSource { get; }
    Area3D LightArea { get; }
    RayCast3D LightRay { get; }

    Vec3 LightPosition => ((Node3D)this).GlobalPosition;

    bool IsActive => LightSource.LightEnergy > 0f;

    float LightEnergy => LightSource.LightEnergy;

    float EffectiveRange =>
        LightSource switch
        {
            OmniLight3D omni => omni.OmniRange,
            SpotLight3D spot => spot.SpotRange,
            _ => 10f,
        };

    void OnReceivedObject(Node3D body);

    void OnRemovedObject(Node3D body);
}
