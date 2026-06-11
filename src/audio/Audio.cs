using System;
using System.Collections.Generic;
using System.Linq;

namespace Yotf;

/// <summary>
/// Manages Sound Effects and Background Music
/// SFX and BGM are lazy loaded and cached
/// </summary>
public partial class Audio : Node
{
    const int SfxPlayerCount = 10;

    static Audio instance = null!; // static singleton instance, global nodes are loaded before the scene tree.
    readonly Dictionary<string, AudioStream> sfxCache = [];
    readonly Dictionary<string, AudioStream> bgmCache = [];

    public static AudioStream GetSfx(string filepath)
    {
        if (instance.sfxCache.TryGetValue(filepath, out var stream))
            return stream;
        try
        {
            instance.sfxCache.Add(filepath, GD.Load<AudioStream>($"{Sfx.path}{filepath}"));
            return instance.sfxCache[filepath];
        }
        catch (Exception e)
        {
            GD.PrintErr($"Audio file {Sfx.path}{filepath} not found in file system: {e}");
            return Sfx.Defualt;
        }
    }

    public static AudioStream GetBgm(string filepath)
    {
        if (instance.bgmCache.TryGetValue(filepath, out var stream))
            return stream;
        try
        {
            instance.bgmCache.Add(filepath, GD.Load<AudioStream>($"{Bgm.path}{filepath}"));
            return instance.bgmCache[filepath];
        }
        catch (Exception e)
        {
            GD.PrintErr($"Audio file {Bgm.path}{filepath} not found in file system: {e}");
            return Bgm.Defualt;
        }
    }

    int bus = AudioServer.GetBusIndex("Master");
    AudioStreamPlayer BgmPlayer = new();
    AudioStreamPlayer[] staticSfxPlayers = new AudioStreamPlayer[SfxPlayerCount];

    public override void _Ready()
    {
        instance = this;
        AddChild(BgmPlayer);
        for (int i = 0; i < SfxPlayerCount; i++)
        {
            staticSfxPlayers[i] = new();
            AddChild(staticSfxPlayers[i]);
        }
        BgmPlayer.ProcessMode = ProcessModeEnum.Always;
    }

    public static void PlayBgm(string bgm, float playbackPosition = 0)
    {
        var music = GetBgm(bgm);
        if (instance.BgmPlayer.Stream == music)
        {
            return;
        }
        instance.BgmPlayer.Stream = music;
        instance.BgmPlayer.Play(playbackPosition);
    }

    public static bool IsPlayingSfx(string sfxName) =>
        instance.staticSfxPlayers.FirstOrDefault(sfxPlayer =>
            sfxPlayer.Stream == GetSfx(sfxName) && sfxPlayer.Playing
        ) != null;

    public static bool IsPlayingBgm(string bgmName) =>
        instance.BgmPlayer.Playing && instance.BgmPlayer.Stream == GetBgm(bgmName);

    public static void PlaySfx(
        string sfxName,
        bool singleStreamOnly = false,
        float playbackPosition = 0
    )
    {
        var sfx = GetSfx(sfxName);
        if (singleStreamOnly)
        {
            var alreadyPlayingStream = instance.staticSfxPlayers.FirstOrDefault(sfxPlayer => //TODO(j) linq iterators are bad for memory allocs, but it's not runnin in a hot loop so we'll fix if it's a problem.
                sfxPlayer.Stream == sfx && sfxPlayer.Playing
            );
            if (alreadyPlayingStream != null)
            {
                return;
            }
        }

        var sfxPlayer = instance.staticSfxPlayers.FirstOrDefault(sfxPlayer =>
            !sfxPlayer.IsPlaying()
        );

        if (sfxPlayer == null)
        {
            GD.PushWarning($"No Free SFX player to play {sfxName}");
            return;
        }

        sfxPlayer.Stream = sfx;
        sfxPlayer.Play(playbackPosition);
    }

    public static void PlaySfx(
        string sfxName,
        AudioStreamPlayer3D positionalPlayer,
        float playbackPosition = 0
    )
    {
        var sfx = GetSfx(sfxName);
        positionalPlayer.Stream = sfx;
        positionalPlayer.Play(playbackPosition);
    }

    public static void PlaySfxFrom(string sfxName, Vec3 position, float playbackPosition = 0)
    {
        var positionalPlayer = new AudioStreamPlayer3D();
        instance.AddChild(positionalPlayer);
        var sfx = GetSfx(sfxName);
        positionalPlayer.Stream = sfx;
        positionalPlayer.Play(playbackPosition);
        positionalPlayer.Finished += () => positionalPlayer.QueueFree();
    }

    public static void StopSfx(string sfxName)
    {
        var sfx = GetSfx(sfxName);
        var sfxStream = instance.staticSfxPlayers.FirstOrDefault(sfxPlayer =>
            sfxPlayer.Stream == sfx && sfxPlayer.Playing
        );
        sfxStream?.Stop();
    }

    public static void SetVolume(float value)
    {
        AudioServer.SetBusVolumeDb(instance.bus, value);
    }

    public static void StopAllSfx()
    {
        Array.ForEach(instance.staticSfxPlayers, (player) => player.Stop());
    }

    public static void PauseBgm()
    {
        instance.BgmPlayer.Stop();
    }
}
