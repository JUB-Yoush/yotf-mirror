using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Hud : Control
{
    public override void _Notification(int what) => this.Notify(what);

    public const int InventorySize = 4;

    Player player = null!;

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
    public required TextureProgressBar InjuryBar { set; get; }

    [Node]
    public required Node3D Gimbal { set; get; }

    [Node]
    public required Node3D GimbalArm { set; get; }

    [Node]
    public required TextureRect Ruler { set; get; }

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

    [Export]
    public float RulerSmoothing = 0.1f;
    private float smoothedSpeed = 0f;
    private float prevDepth;

    public Vector2 slotMinSize = new(200, 200);
    public Vector2 slotMaxSize = new(250, 250);
    ShaderMaterial barometerShader = null!;

    public override void _Ready()
    {
        player = this.SceneRoot().GetNode<Player>()!;
        prevDepth = player.Depth;
        barometerShader = (ShaderMaterial)Ruler.Material;
        // PlayerController.StateChanged += OnPlayerStateChanged;
    }

    // private void OnPlayerStateChanged(IPlayerState prevState, IPlayerState newState)
    // {
    //     if (prevState is SwimmingState)

    // }

    public override void _Process(double delta)
    {
        RotateGimbalToCam();
        UpdateBarometer((float)delta);
    }

    void UpdateBarometer(float delta)
    {
        // Weighted Exponential Averaging
        var instantSpeed = (player.Depth - prevDepth) / delta;
        prevDepth = player.Depth;
        var alpha = 1f - Mathf.Exp(-delta / RulerSmoothing);
        smoothedSpeed = alpha * instantSpeed + (1f - alpha) * smoothedSpeed;
        var shaderSpeed = new Vector2(0, smoothedSpeed);
        barometerShader.SetShaderParameter("scroll_speed", shaderSpeed / 100);
    }

    public void SetItemSlot(int index, Texture2D img)
    {
        InventoryIcons[index].Texture = img;
    }

    public void SelectSlot(int index)
    {
        for (int i = 0; i < InventorySize; i++)
            InventoryIcons[i]?.CustomMinimumSize = i == index ? slotMaxSize : slotMinSize;
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
