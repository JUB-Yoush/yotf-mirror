using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Hud : Control
{
	public override void _Notification(int what) => this.Notify(what);

	public const int InventorySize = 4;

	public Camera3D Camera = null!;
	public Label OxygenLabel = null!;
	public Label BatteryLabel = null!;
	public Label MoneyLabel = null!;
	public Label PhotoLabel = null!;

	public TextureProgressBar BatteryBar = null!;
	public TextureProgressBar OxygenBar = null!;

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

	public Node3D OrientationGimbal = null!;
	public Node3D CameraArm = null!;

	public override void _Ready()
	{
		Camera = GetNode<Camera3D>("%Camera3D");
		OxygenLabel = GetNode<Label>("DebugPanel/OxygenLabel");
		BatteryLabel = GetNode<Label>("DebugPanel/BatteryLabel");
		MoneyLabel = GetNode<Label>("DebugPanel/MoneyLabel");
		PhotoLabel = GetNode<Label>("DebugPanel/PhotoLabel");

		BatteryBar = GetNode<TextureProgressBar>("%BatteryOxygenBars/BatteryBar");
		OxygenBar = GetNode<TextureProgressBar>("%BatteryOxygenBars/OxygenBar");
		for (int i = 0; i < InventorySize; i++)
		{
			InventoryIcons[i] = GetNode<TextureRect>($"%InventoryIcons/Slot{i}/Border/{i}");
		}
		OrientationGimbal = GetNode<Node3D>(
            "%OrientationGimbal/OrientationGimbalViewport/OrientationGimbal"
		);
		CameraArm = OrientationGimbal.GetParent().GetNode<Node3D>("Arm");
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
			X = Camera.GlobalRotation.X,
			Y = -Camera.GlobalRotation.Y,
			Z = Camera.GlobalRotation.Z,
		};
	}
}
