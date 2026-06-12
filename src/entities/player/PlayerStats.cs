using System;
using System.Diagnostics;
using Godot;
using static Yotf.Disposable;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class PlayerStats : Node
{
    public override void _Notification(int what) => this.Notify(what);

    private Player player = null!;

    public Action<int>? GalleryScoreUpdated;

    public bool isDead = false;

    private bool oxygenWarningGiven = false;
    private bool batteryWarningGiven = false;

    [Node]
    public required Hud HUD { set; get; }

    [Export]
    public float OxygenUseRate = 0f;

    [Export]
    public float LowOxygenPercentage = .5f;

    [Export]
    public float LowBatteryPercentage = .5f;

    [Export]
    int startingMoney = 2000;

    public float Injuries
    {
        get;
        set
        {
            field = Math.Clamp(value, 0, MaxOxygen);
            HUD.InjuryBar.Value = Mathf.Floor(field);
        }
    }

    public float MaxOxygen
    {
        get;
        set
        {
            field = value;
            HUD.OxygenBar.MaxValue = field;
        }
    } = 100;
    public float MaxBattery
    {
        get;
        set
        {
            field = value;
            HUD.BatteryBar.MaxValue = field;
        }
    } = 100;
    public float Oxygen
    {
        get;
        set
        {
            field = Math.Clamp(value, 0, MaxOxygen - Injuries);
            HUD?.OxygenBar.Value = field;

            if (field / MaxOxygen > LowOxygenPercentage)
            {
                oxygenWarningGiven = false;
            }

            if (field / MaxOxygen <= LowOxygenPercentage && !oxygenWarningGiven)
            {
                oxygenWarningGiven = true;
                player?.MakeAlert("ALERT: LOW OXYGEN");
            }
            if (field == 0)
            {
                isDead = true;
                Drown();
            }
        }
    }
    public float Battery
    {
        get;
        set
        {
            field = Math.Clamp(value, 0, MaxBattery);
            HUD?.BatteryLabel?.Text = $"Battery: {value}/{MaxBattery}";
            HUD?.BatteryBar.Value = value;

            if (field / MaxBattery > LowBatteryPercentage)
            {
                batteryWarningGiven = false;
            }
            if (field / MaxBattery <= LowBatteryPercentage && !batteryWarningGiven)
            {
                batteryWarningGiven = true;
                player?.MakeAlert("ALERT: LOW BATTERY");
            }

            if (field == 0 && HUD != null)
            {
                Audio.PlaySfx(Sfx.PowerDown);
                var shockTween = CreateTween();
                shockTween.AnimateProperty(
                    HUD,
                    Control.PropertyName.Modulate,
                    new Color(0xffffff00),
                    .5f
                );
            }
            else if (HUD != null)
            {
                var shockTween = CreateTween();
                shockTween.AnimateProperty(
                    HUD,
                    Control.PropertyName.Modulate,
                    new Color(0xffffffff),
                    .5f
                );
            }
        }
    }
    public int Money
    {
        get;
        set
        {
            field = value;
            HUD?.MoneyLabel?.Text = $"Money: {value}";
            foreach (var lab in this.SceneRoot().GetNodes<Lab>())
            {
                lab.ShopKiosk.ScoreLabel.Text = $"{value:D6}";
            }
        }
    }
    public int TotalGalleryScore
    {
        get;
        set
        {
            field = value;
            HUD?.PhotoLabel?.Text = $"Photo Points: {value}";
            foreach (var lab in this.SceneRoot().GetNodes<Lab>())
            {
                lab.PhotoTerminal.LabelText = $"{value:D6}";
            }
            GalleryScoreUpdated?.Invoke(value);
        }
    }

    static int maxFilm = 10;
    public static int MaxFilm
    {
        set { maxFilm = value; }
        get => maxFilm;
    }

    static float maxZoom = 10;
    public static float MaxZoom
    {
        set { maxZoom = value; }
        get => maxZoom;
    }

    static float extraSwimSpeed = 3;
    public static float ExtraSwimSpeed
    {
        set { extraSwimSpeed = value; }
        get => extraSwimSpeed;
    }

    public override void _Ready()
    {
        Money = startingMoney;
        player = GetParent<Player>();
        HUD.OxygenBar.MaxValue = MaxOxygen;
        HUD.BatteryBar.MaxValue = MaxBattery;
        Oxygen = MaxOxygen;
        Battery = MaxBattery;
        TotalGalleryScore = 10;
    }

    public void SpendOxygen(double delta)
    {
        if (Lab.CurrentLab is null)
            return;
            
        OxygenUseRate = Lab.CurrentLab!.OxygenScale;
        Oxygen = Math.Max(Oxygen - (float)(OxygenUseRate * delta), 0);
    }

    public void Drown()
    {
        HUD.DeathText.ProcessMode = ProcessModeEnum.Always;
        HUD.ScreenColor.ProcessMode = ProcessModeEnum.Always;
        this.ProcessMode = ProcessModeEnum.Always;
        GetTree().Paused = true;
        var fadeRect = player.HUD.ScreenColor;
        fadeRect.Visible = true;
        var tween = CreateTween();
        tween.AnimateProperty(fadeRect, ColorRect.PropertyName.Color, new Color(0, 0, 0, 1), 3f);
        tween.AnimateProperty(
            HUD.DeathText,
            TextureRect.PropertyName.Modulate,
            new Color(1, 1, 1, 1),
            1f,
            true
        );
    }

    internal void RestoreOxygen()
    {
        Oxygen = MaxOxygen - Injuries;
    }

    public void RestoreStat(Restore restore, float amount)
    {
        Debug.Assert(restore != Restore.None, "None restorable item passed into restore function");
        Log.PrintLn($"restoring {restore} by {amount}");
        switch (restore)
        {
            case Restore.Oxygen:
                Oxygen += amount;
                Audio.PlaySfx(Sfx.Oxygen);
                break;

            case Restore.Battery:
                Audio.PlaySfx(Sfx.PowerUp);
                Battery += amount;
                break;

            case Restore.Injuries:
                Injuries += amount;
                break;
        }
    }
}
