using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class BGM
{
    //load BGM
    public static AudioStream AquaticAmbience = GD.Load<AudioStream>(
        "res://assets/audio/sfx/Aquatic_Ambience.mp3"
    );
}

public static class SFX
{
    //should either be a hashmap or lazy-loaded
    public static AudioStream MenuOpen = GD.Load<AudioStream>(
        "res://assets/audio/sfx/MenuOpen.mp3"
    );
    public static AudioStream MenuClose = GD.Load<AudioStream>(
        "res://assets/audio/sfx/MenuClose.mp3"
    );
    public static AudioStream UIDecrease = GD.Load<AudioStream>(
        "res://assets/audio/sfx/UI_Decrease.mp3"
    );

    public static AudioStream UIIncrease = GD.Load<AudioStream>(
        "res://assets/audio/sfx/UI_Increase.mp3"
    );

    public static AudioStream UISelect = GD.Load<AudioStream>(
        "res://assets/audio/sfx/UI_Select.mp3"
    );

    public static AudioStream Ping = GD.Load<AudioStream>(
        "res://assets/audio/sfx/Ping.mp3"
    );

    public static AudioStream WaterStep1 = GD.Load<AudioStream>(
        "res://assets/audio/sfx/Water_Step_1.mp3"
    );
    public static AudioStream WaterStep2 = GD.Load<AudioStream>(
        "res://assets/audio/sfx/Water_Step_2.mp3"
    );

    public static AudioStream WaterStep3 = GD.Load<AudioStream>(
        "res://assets/audio/sfx/Water_Step_3.mp3"
    );

    public static AudioStream OxygenFill = GD.Load<AudioStream>(
        "res://assets/audio/sfx/Oxygen_Fill.mp3"
    );
}

public partial class AudioManager : Node
{
    const int SFX_PLAYER_COUNT = 5;
    int bus = AudioServer.GetBusIndex("Master");
    AudioStreamPlayer BgmPlayer = new();
    List<AudioStreamPlayer> SfxPlayers = [];

    public static AudioManager Ref = null!; // static singleton instance, global nodes are loaded before the scene tree.

    public override void _Ready()
    {
        Ref = this;
        AddChild(BgmPlayer);
        for (int i = 0; i < SFX_PLAYER_COUNT; i++)
        {
            var sfxPlayer = new AudioStreamPlayer();
            AddChild(sfxPlayer);
            SfxPlayers.Add(sfxPlayer);
        }
        BgmPlayer.ProcessMode = ProcessModeEnum.Always;
    }

    public static void PlayMusic(AudioStream music)
    {
        if (Ref.BgmPlayer.Stream == music)
        {
            return;
        }
        Ref.BgmPlayer.Stream = music;
        Ref.BgmPlayer.Play();
    }

    public static void PlaySfx(AudioStream sfx, bool singleStreamOnly = false)
    {
        // only one stream playing at a time
        if (singleStreamOnly)
        {
            var alreadyPlayingStream = Ref.SfxPlayers.FirstOrDefault(sfxPlayer =>
                sfxPlayer.Stream == sfx && sfxPlayer.Playing
            );
            if (alreadyPlayingStream != null)
            {
                return;
            }
        }

        var sfxPlayer = Ref.SfxPlayers.FirstOrDefault(sfxPlayer => !sfxPlayer.IsPlaying());

        if (sfxPlayer == null)
        {
            return;
        }

        sfxPlayer.Stream = sfx;
        sfxPlayer.Play();
    }

    public static void StopSfx(AudioStream sfx)
    {
        var sfxStream = Ref.SfxPlayers.FirstOrDefault(sfxPlayer =>
            sfxPlayer.Stream == sfx && sfxPlayer.Playing
        );
        sfxStream?.Stop();
    }

    public static void SetVolume(float value)
    {
        AudioServer.SetBusVolumeDb(Ref.bus, value);
    }

    public static void StopAll()
    {
        Ref.SfxPlayers.ForEach(SfxPlayer => SfxPlayer.Stop());
    }

    public static void PauseMusic()
    {
        Ref.BgmPlayer.Stop();
    }
}
