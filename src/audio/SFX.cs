using System.Collections.Generic;

namespace Yotf;

public enum SFX
{
    CameraShutter,
}

public static class SFXLoader
{
    static readonly Dictionary<SFX, AudioStream> cache = [];
    public static readonly Dictionary<SFX, string> map = new()
    {
        { SFX.CameraShutter, "res://assets/audio/sfx/photo.ogg" },
    };

    public static AudioStream Get(SFX sfx)
    {
        if (cache.TryGetValue(sfx, out var stream))
            return stream;
        cache.Add(sfx, GD.Load<AudioStream>(map[sfx]));
        return cache[sfx];
    }
}
