using System.Collections.Generic;

namespace Yotf;

public enum SFX
{
    CameraShutter,
}

public static class SFXLoader
{
    public static readonly Dictionary<SFX, AudioStream> Map = new()
    {
        { SFX.CameraShutter, GD.Load<AudioStream>("res://assets/audio/sfx/photo.ogg") },
    };
}
