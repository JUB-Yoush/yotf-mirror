using System;
using Godot;

namespace Yotf;

public partial class Hud : Control
{
    public Label OxygenLabel = null!;
    public Label BatteryLabel = null!;
    public Label MoneyLabel = null!;
    public Label PhotoLabel = null!;

    public override void _Ready()
    {
        OxygenLabel = GetNode<Label>("DebugPanel/OxygenLabel");
        BatteryLabel = GetNode<Label>("DebugPanel/BatteryLabel");
        MoneyLabel = GetNode<Label>("DebugPanel/MoneyLabel");
        PhotoLabel = GetNode<Label>("DebugPanel/PhotoLabel");
    }

    public void SetOxygenText(float value)
    {
        OxygenLabel?.Text = $"{value}";
    }
}
