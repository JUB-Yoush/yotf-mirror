namespace Yotf;

public interface IHearNoise
{
    public void OnNoiseHeard(Vector3 position, float dB, SFX noise);
}
