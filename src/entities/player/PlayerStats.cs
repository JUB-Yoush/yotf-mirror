using System;
using Godot;

namespace Yotf;

public partial class PlayerStats : Node
{
    Hud playerHud = null!;
    float maxOxygen = 100;
    float maxBattery = 100;
    float Oxygen
    {
        get;
        set
        {
            field = value;
            playerHud?.MoneyLabel?.Text = $"Money: {value}";
        }
    }
    float Battery
    {
        get;
        set
        {
            field = value;
            playerHud?.MoneyLabel?.Text = $"Money: {value}";
        }
    }
    int Money
    {
        get;
        set
        {
            field = value;
            playerHud?.MoneyLabel?.Text = $"Money: {value}";
        }
    }
    int TotalGalleryScore
    {
        get;
        set
        {
            field = value;
            playerHud?.PhotoLabel?.Text = $"Photo Points: {value}";
        }
    }

    //PlayerController player = null!;

    public override void _Ready()
    {
        playerHud = GetParent().GetNode<Hud>("HUD");
        Oxygen = maxOxygen;
        Battery = maxBattery;
        Money = 0;
        TotalGalleryScore = 0;
    }
}
