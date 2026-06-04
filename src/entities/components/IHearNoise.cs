namespace Yotf;

/// <summary>
/// Bodies that implement this interface must be on the NoiseListener Layer
/// </summary>
public interface IHearNoise
{
    public void OnNoiseHeard(Node3D NoiseSource, float dB, string noise);
}
