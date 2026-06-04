using System;
using System.Collections.Generic;
using System.Linq;

namespace Yotf;

public static class SFX
{
    public static readonly AudioStream Defualt = GD.Load<AudioStream>(
        "res://assets/audio/sfx/photo.ogg"
    );
    public const string CameraShutter = "res://assets/audio/sfx/photo.ogg";
    public const string Firecracker = "res://assets/audio/sfx/firecracker.ogg";
}

public static class BGM
{
    public static readonly AudioStream Defualt = GD.Load<AudioStream>(
        "res://assets/audio/bgm/plum_fairy.ogg"
    );
    public const string PlumFairy = "res://assets/audio/bgm/plum_fairy.ogg";
}

/// <summary>
/// Manages Sound Effects and Background Music
/// SFX and BGM are lazy loaded and cached
/// </summary>
public partial class Audio : Node
{
    const int SfxPlayerCount = 10;

    static Audio Instance = null!; // static singleton instance, global nodes are loaded before the scene tree.
    readonly Dictionary<string, AudioStream> cache = [];

    public static AudioStream Get(string sfx)
    {
        if (Instance.cache.TryGetValue(sfx, out var stream))
            return stream;
        try
        {
            Instance.cache.Add(sfx, GD.Load<AudioStream>(sfx));
            return Instance.cache[sfx];
        }
        catch (Exception e)
        {
            GD.PrintErr($"SFX {sfx} not found in file system: {e}");
            return SFX.Defualt;
        }
    }

    int bus = AudioServer.GetBusIndex("Master");
    AudioStreamPlayer BgmPlayer = new();
    AudioStreamPlayer[] SfxPlayers = new AudioStreamPlayer[SfxPlayerCount];

    public override void _Ready()
    {
        Instance = this;
        AddChild(BgmPlayer);
        for (int i = 0; i < SfxPlayerCount; i++)
        {
            SfxPlayers[i] = new();
            AddChild(SfxPlayers[i]);
        }
        BgmPlayer.ProcessMode = ProcessModeEnum.Always;
    }

    public static void PlayBgm(string bgm, float playbackPosition = 0)
    {
        var music = Get(bgm);
        if (Instance.BgmPlayer.Stream == music)
        {
            return;
        }
        Instance.BgmPlayer.Stream = music;
        Instance.BgmPlayer.Play(playbackPosition);
    }

    public static void PlaySfx(
        string sfxName,
        bool singleStreamOnly = false,
        float playbackPosition = 0
    )
    {
        var sfx = Get(sfxName);
        if (singleStreamOnly)
        {
            var alreadyPlayingStream = Instance.SfxPlayers.FirstOrDefault(sfxPlayer => //TODO(j) linq iterators are bad for memory allocs, but it's not runnin in a hot loop so we'll fix if it's a problem.
                sfxPlayer.Stream == sfx && sfxPlayer.Playing
            );
            if (alreadyPlayingStream != null)
            {
                return;
            }
        }

        var sfxPlayer = Instance.SfxPlayers.FirstOrDefault(sfxPlayer => !sfxPlayer.IsPlaying());

        if (sfxPlayer == null)
        {
            GD.PushWarning($"No Free SFX player to play {sfxName}");
            return;
        }

        sfxPlayer.Stream = sfx;
        sfxPlayer.Play(playbackPosition);
    }

    public static void PlaySfx(
        AudioStreamPlayer3D positionalPlayer,
        string sfxName,
        float playbackPosition = 0
    )
    {
        var sfx = Get(sfxName);
        positionalPlayer.Stream = sfx;
        positionalPlayer.Play(playbackPosition);
    }

    public static void StopSfx(string sfxName)
    {
        var sfx = Get(sfxName);
        var sfxStream = Instance.SfxPlayers.FirstOrDefault(sfxPlayer =>
            sfxPlayer.Stream == sfx && sfxPlayer.Playing
        );
        sfxStream?.Stop();
    }

    public static void SetVolume(float value)
    {
        AudioServer.SetBusVolumeDb(Instance.bus, value);
    }

    public static void StopAllSfx()
    {
        Array.ForEach(Instance.SfxPlayers, (player) => player.Stop());
    }

    public static void PauseBgm()
    {
        Instance.BgmPlayer.Stop();
    }
}
