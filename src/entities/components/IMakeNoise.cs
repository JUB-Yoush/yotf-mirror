using System.Linq;

namespace Yotf;

/// <summary>
/// Nodes that implement this interface have sound effects that nodes that implement the listening Interface can listen+react to.
/// Decibels of noise scale from about 30 to 140.
/// </summary>
public interface IMakeNoise
{
    AudioStreamPlayer3D NoiseSource { get; }

    public static void MakeNoise(IMakeNoise node, float dB, string sfx, int radius = -1)
    {
        // play sound
        Audio.PlaySfx(node.NoiseSource, sfx, 0);
        // check who heard it
        var streamPlayer = node.NoiseSource; // TODO (j) make this it's own scene to ensure the dependencies are there.
        var audioArea = streamPlayer.GetNode<Area3D>()!;

        //TODO (j) area should only be enabled when the sound is playing.
        foreach (var body in audioArea.GetOverlappingBodies())
        {
            Log.PrintLn(body.Name);
            var listener = (IHearNoise)body;
            listener.OnNoiseHeard(audioArea, dB, sfx);
        }
    }

    public int GetRadiusFromdB(float dB)
    {
        return 5; // TODO(j) formula for the size of the noise radius based on how loud the noise is
    }
}
