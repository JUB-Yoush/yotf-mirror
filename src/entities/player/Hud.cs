using System;
using Godot;

namespace Yotf;

public partial class Hud : Control
{

    public Label OxygenLabel = null!;
    public Label BatteryLabel = null!;
    public Label MoneyLabel = null!;
    public Label PhotoLabel = null!;

    public TextureProgressBar HealthBar = null;
    public TextureProgressBar OxygenBar = null;

    public TextureRect[] InventoryIcons = new TextureRect[4];

    public override void _Ready()
    {
        OxygenLabel = GetNode<Label>("DebugPanel/OxygenLabel");
        BatteryLabel = GetNode<Label>("DebugPanel/BatteryLabel");
        MoneyLabel = GetNode<Label>("DebugPanel/MoneyLabel");
        PhotoLabel = GetNode<Label>("DebugPanel/PhotoLabel");

       HealthBar = GetNode<TextureProgressBar>("%HealthOxygenBars/HealthBar");
       OxygenBar = GetNode<TextureProgressBar>("%HealthOxygenBars/OxygenBar");


       for(int i = 0; i < 4; i++)
        {
       
            InventoryIcons[i] = GetNode<TextureRect>($"%InventoryIcons/Slot{i}/Border/{i}"); //retrieve location of each TextureRect under InventoryIcons
         
        }   


    }

    public void SetOxygenText(float value)
    {
        OxygenLabel?.Text = $"{value}";
    }

    public void SetOxygen(float value)
    {
       OxygenBar.Value += value; 
    }

    public void SetHealth(float value)
    {
        HealthBar.Value += value;
    }
    public void SetItemSlot(int index, Texture2D img)
    {
        InventoryIcons[index].Texture = img;
    }
}
