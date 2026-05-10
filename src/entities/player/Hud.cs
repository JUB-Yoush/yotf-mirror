using System;
using Godot;

namespace Yotf;

public partial class Hud : Control
{
    public static int InventorySize = 4;
   
    public Camera3D Camera = null!;
    public Label OxygenLabel = null!;
    public Label BatteryLabel = null!;
    public Label MoneyLabel = null!;
    public Label PhotoLabel = null!;

    public TextureProgressBar BatteryBar;
    public TextureProgressBar OxygenBar;

    public TextureRect[] InventoryIcons = new TextureRect[InventorySize];
     public Vector2 slotMinSize = new Vector2(200, 200);
     public Vector2 slotMaxSize = new Vector2(250, 250);

    public Node3D OrientationGimbal;

    public override void _Ready()
    {
     
        Camera = GetNode<Camera3D>("%Camera3D");
        OxygenLabel = GetNode<Label>("DebugPanel/OxygenLabel");
        BatteryLabel = GetNode<Label>("DebugPanel/BatteryLabel");
        MoneyLabel = GetNode<Label>("DebugPanel/MoneyLabel");
        PhotoLabel = GetNode<Label>("DebugPanel/PhotoLabel");

       BatteryBar = GetNode<TextureProgressBar>("%BatteryOxygenBars/BatteryBar");
       OxygenBar = GetNode<TextureProgressBar>("%BatteryOxygenBars/OxygenBar");
     for(int i = 0; i < 4; i++)
        {
       
            InventoryIcons[i] = GetNode<TextureRect>($"%InventoryIcons/Slot{i}/Border/{i}"); //retrieve location of each TextureRect under InventoryIcons
         
        }   

        OrientationGimbal = GetNode<Node3D>("%OrientationGimbal/OrientationGimbalViewport/OrientationGimbal");
    
      
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

    public void SetBattery(float value)
    {
        BatteryBar.Value += value;
    }
    public void SetItemSlot(int index, Texture2D img)
    {
        InventoryIcons[index].Texture = img;
    }

    public void SelectSlot(int index)
    {
       
        for(int i = 0; i < 4; i++)
        {
            PanelContainer slotContainer = InventoryIcons[i].GetParent<PanelContainer>();
       
            if (i == index) { //selected slot becomes bigger
              slotContainer.CustomMinimumSize = slotMaxSize;
              
                }
            else slotContainer.CustomMinimumSize = slotMinSize;
              
        }
    }

    public void ClearSlots()
    {
        foreach(TextureRect text in InventoryIcons) {
            text.Texture = null;
        }
    }

    public void RotateGimbalToCam()
    {
       
        
        OrientationGimbal.GlobalRotation = Camera.GlobalRotation;
    }
}
