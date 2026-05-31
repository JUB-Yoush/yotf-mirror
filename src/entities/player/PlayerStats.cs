using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class PlayerStats : Node
{
    public override void _Notification(int what) => this.Notify(what);

    private PlayerController player = null!;

    public Action<int> GalleryScoreUpdated;

    [Node]
    public required Hud HUD { set; get; }

    [Export]
    public float OxygenUseRate = 0f;

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
    }
    public float MaxBattery
    {
        get;
        set
        {
            field = value;
            HUD.BatteryBar.MaxValue = field;
        }
    }
    public float Oxygen
    {
        get;
        set
        {
            field = Math.Clamp(value, 0, MaxOxygen - Injuries);
            HUD?.OxygenBar.Value = field;
            if (field == 0)
                Drown();
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

    public override void _Ready()
    {
        player = GetParent<PlayerController>();
        HUD.OxygenBar.MaxValue = MaxOxygen;
        HUD.BatteryBar.MaxValue = MaxBattery;
        MaxOxygen = 100;
        MaxBattery = 100;
        Oxygen = MaxOxygen;
        Battery = MaxBattery;
        Money = 100;
        TotalGalleryScore = 0;
    }

    public void SpendOxygen(double delta)
    {
        //TODO (j) we need to come up wtih some normalized depth value relative to the underwater lab or smthn
        OxygenUseRate = Lab.CurrentLab!.OxygenScale;
        Oxygen = Math.Max(Oxygen - (float)(OxygenUseRate * delta), 0);
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionPressed("roll"))
        {
            Injuries += 10 * (float)delta;
        }
    }

    public void Drown()
    {
        var fadeRect = GetParent().GetNode<ColorRect>("%FadeToBlack");
        fadeRect.Visible = true;
        var tween = CreateTween();
        tween.LerpProperty(fadeRect, ColorRect.PropertyName.Color, new Color(0, 0, 0, 1), 1f);
    }

    internal void RestoreOxygen()
    {
        Oxygen = MaxOxygen - Injuries;
    }
}
