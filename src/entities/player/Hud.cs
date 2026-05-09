using System;
using Godot;

namespace Yotf;

public partial class Hud : Control
{

    public Camera3D Camera = null!;
    public Label OxygenLabel = null!;
    public Label BatteryLabel = null!;
    public Label MoneyLabel = null!;
    public Label PhotoLabel = null!;

    public TextureProgressBar HealthBar = null;
    public TextureProgressBar OxygenBar = null;

    public TextureRect[] InventoryIcons = new TextureRect[4];

    public Node3D OrientationGimbal = null;

    public override void _Ready()
    {
        Camera = GetNode<Camera3D>("%Camera3D");
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

        OrientationGimbal = GetNode<Node3D>("%OrientationGimbal/OrientationGimbalViewport/orientation_gimbal");

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

    public void RotateGimbalToCam()
    {
        Vector3 targetRotation = new Vector3(Camera.GlobalRotation.X,OrientationGimbal.Rotation.Y, OrientationGimbal.Rotation.Z); //only rotate on X axis
        OrientationGimbal.Rotation = -targetRotation;

    }
}
