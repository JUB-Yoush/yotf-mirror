using System;
using Godot;

namespace Yotf;

public partial class PlayerStats : Node
{
    private Hud playerHud = null!;
    public float maxOxygen = 100;
    public float maxBattery = 100;
    public float Oxygen
    {
        get;
        set
        {
            field = Math.Max(value, 0);
            playerHud?.OxygenLabel?.Text = $"O2: {value}/{maxOxygen}";
            playerHud.SetOxygen(value);
        }
    }
    public float Battery
    {
        get;
        set
        {
            field = Math.Max(value, 0);
            playerHud?.BatteryLabel?.Text = $"Battery: {value}/{maxBattery}";
            playerHud.SetBattery(value);
        }
    }
    public int Money
    {
        get;
        set
        {
            field = value;
            playerHud?.MoneyLabel?.Text = $"Money: {value}";
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
       playerHud.OxygenBar.MaxValue = maxOxygen;
       playerHud.BatteryBar.MaxValue = maxBattery;
        Oxygen = maxOxygen;
        Battery = maxBattery;
        Money = 100;
        TotalGalleryScore = 0;
    }
}
