using System;
using Godot;

namespace Yotf;

public partial class PlayerStats : Node
{
    private Hud playerHud = null!;

    [Export]
    public float OxygenUseRate = 3f;
    public float MaxOxygen
    {
        get;
        set
        {
            field = value;
            playerHud.OxygenBar.MaxValue = field;
        }
    }
    public float MaxBattery
    {
        get;
        set
        {
            field = value;
            playerHud.BatteryBar.MaxValue = field;
        }
    }
    public float Oxygen
    {
        get;
        set
        {
            field = Math.Clamp(value, 0, MaxOxygen);
            playerHud?.OxygenLabel?.Text = $"O2: {value}/{MaxOxygen}";
            playerHud?.OxygenBar.Value = value;
            if (value == 0)
                Drown();
        }
    }
    public float Battery
    {
        get;
        set
        {
            field = Math.Clamp(value, 0, MaxBattery);
            playerHud?.BatteryLabel?.Text = $"Battery: {value}/{MaxBattery}";
            playerHud?.BatteryBar.Value = value;
        }
    }
    public int Money
    {
        get;
        set
        {
            field = value;
            playerHud?.MoneyLabel?.Text = $"Money: {value}";
            var photoTerminal = GetTree()
                .CurrentScene.GetNodeOrNull<PhotoTerminal>("%PhotoTerminal");
            photoTerminal.LabelText = $"{value:D6}";
        }
    }
    public int TotalGalleryScore
    {
        get;
        set
        {
            field = value;
            playerHud?.PhotoLabel?.Text = $"Photo Points: {value}";
        }
    }

    public override void _Ready()
    {
        playerHud = GetNode<Hud>("%HUD");
        playerHud.OxygenBar.MaxValue = MaxOxygen;
        playerHud.BatteryBar.MaxValue = MaxBattery;
        MaxOxygen = 100;
        MaxBattery = 100;
        Oxygen = MaxOxygen;
        Battery = MaxBattery;
        Money = 100;
        TotalGalleryScore = 0;
    }

    public void SpendOxygen(double delta)
    {
        Oxygen = Math.Max(Oxygen - (float)(OxygenUseRate * delta), 0);
    }

    public void Drown()
    {
        var fadeRect = GetParent().GetNode<ColorRect>("%FadeToBlack");
        fadeRect.Visible = true;
        var tween = CreateTween();
        tween.TweenProperty(fadeRect, ColorRect.PropertyName.Color, new Color(0, 0, 0, 1), 1f);
    }
}
