using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Hud : Control
{
    public override void _Notification(int what) => this.Notify(what);

    public const int InventorySize = 4;

    public required Camera3D PlayerCam { set; get; }

    [Node]
    public required Label OxygenLabel { set; get; }

    [Node]
    public required Label BatteryLabel { set; get; }

    [Node]
    public required Label MoneyLabel { set; get; }

    [Node]
    public required Label PhotoLabel { set; get; }

    [Node("%StatBarContainer/BatteryBar")]
    public required TextureProgressBar BatteryBar { set; get; }

    [Node("%StatBarContainer/OxygenBar")]
    public required TextureProgressBar OxygenBar { set; get; }

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

    [Node("%OrientationGimbal/OrientationGimbalViewport/OrientationGimbal")]
    public required Node3D OrientationGimbal { set; get; }

    [Node("%OrientationGimbal/OrientationGimbalViewport/Arm")]
    public required Node3D CameraArm { set; get; }

    public override void _Ready()
    {
        PlayerCam = GetNode<Camera3D>("../../../../CameraManager/Camera3D");
    }

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
        CameraArm.GlobalRotation = CameraArm.GlobalRotation with
        {
            X = PlayerCam.GlobalRotation.X,
            Y = -PlayerCam.GlobalRotation.Y,
            Z = PlayerCam.GlobalRotation.Z,
        };
    }
}
