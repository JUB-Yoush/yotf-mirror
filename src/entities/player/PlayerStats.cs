using System;
using Godot;

namespace Yotf;

public partial class PlayerStats : Node
{
    private Hud playerHud = null!;
    private float maxOxygen = 100;
    private float maxBattery = 100;
    public float Oxygen
    {
        get;
        set
        {
            field = Math.Max(value, 0);
            playerHud?.OxygenLabel?.Text = $"O2: {value}/{maxOxygen}";
        }
    }
    public float Battery
    {
        get;
        set
        {
            field = Math.Max(value, 0);
            playerHud?.BatteryLabel?.Text = $"Battery: {value}/{maxBattery}";
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
        playerHud = GetParent().GetNode<Hud>("HUD");
        Oxygen = maxOxygen;
        Battery = maxBattery;
        Money = 100;
        TotalGalleryScore = 0;
    }
}
