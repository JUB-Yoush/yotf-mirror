using System.Linq;

namespace Yotf;

/// <summary>
/// Nodes that implement this interface have sound effects that nodes that implement the listening Interface can listen+react to.
/// Decibels of noise scale from about 30 to 140.
/// </summary>
public interface IMakeNoise
{
    static readonly PackedScene AudioArea = GD.Load<PackedScene>(
        "res://src/entities/gadgets/audio_range.tscn"
    );

    AudioStreamPlayer3D NoiseSource { get; }

    public static void MakeNoise(IMakeNoise node, float dB, string sfx, int radius = -1)
    {
        // play sound
        Audio.PlaySfx(node.NoiseSource, sfx, 0);
        var streamPlayer = node.NoiseSource;
        streamPlayer.Stream = Audio.Get(sfx);
        streamPlayer.Play();

        // check who heard it
        //radius = radius == -1 ? GetRadiusFromdB(dB) : radius;
        var audioArea = streamPlayer.GetNode<Area3D>()!;
        var shape = audioArea.GetNode<CollisionShape3D>()!;

        //TODO (j) area should only be enabled when the sound is playing.
        foreach (IHearNoise listener in audioArea.GetOverlappingBodies().Cast<IHearNoise>())
        {
            listener.OnNoiseHeard(audioArea, dB, sfx);
        }
    }

    public int GetRadiusFromdB(float dB)
    {
        return 5; // TODO(j) formula for the size of the noise radius based on how loud the noise is
    }
}
