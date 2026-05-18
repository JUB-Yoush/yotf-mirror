using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Hud : Control
{
    public override void _Notification(int what) => this.Notify(what);

    public const int InventorySize = 4;

    [Node]
    public required Camera3D Camera { set; get; }

    [Node]
    public required Label OxygenLabel { set; get; }

    [Node]
    public required Label BatteryLabel { set; get; }

    [Node]
    public required Label MoneyLabel { set; get; }

    [Node]
    public required Label PhotoLabel { set; get; }

    [Node]
    public required TextureProgressBar BatteryBar { set; get; }

    [Node]
    public required TextureProgressBar OxygenBar { set; get; }

    [Node]
    public required Node3D Gimbal { set; get; }

    [Node]
    public required Node3D GimbalArm { set; get; }

    public TextureRect[] InventoryIcons
    {
        get
        {
            field = new TextureRect[InventorySize];
            for (int i = 0; i < InventorySize; i++)
                field[i] = GetNode<TextureRect>($"%InventoryIcons/Slot{i}/Border/{i}");
            return field;
        }
    }

    public Vector2 slotMinSize = new(200, 200);
    public Vector2 slotMaxSize = new(250, 250);

    public override void _Process(double delta)
    {
        RotateGimbalToCam();
    }

    public void SetOxygenText(float value)
    {
        OxygenLabel?.Text = $"{value}";
    }

    public void SetOxygen(float value)
    {
        OxygenBar.Value = value;
    }

    public void SetBattery(float value)
    {
        BatteryBar.Value = value;
    }

    public void SetItemSlot(int index, Texture2D img)
    {
        InventoryIcons[index].Texture = img;
    }

    public void SelectSlot(int index)
    {
        for (int i = 0; i < InventorySize; i++)
        {
            var slotContainer = InventoryIcons[i]?.GetParent<PanelContainer>();

            if (i == index)
            {
                slotContainer?.CustomMinimumSize = slotMaxSize;
            }
            else
                slotContainer?.CustomMinimumSize = slotMinSize;
        }
    }

    public void ClearSlots()
    {
        foreach (var text in InventoryIcons)
        {
            text?.Texture = null;
        }
    }

    public void RotateGimbalToCam()
    {
        GimbalArm.GlobalRotation = GimbalArm.GlobalRotation with
        {
            X = Camera.GlobalRotation.X,
            Y = -Camera.GlobalRotation.Y,
            Z = Camera.GlobalRotation.Z,
        };
    }
}
