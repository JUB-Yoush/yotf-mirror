using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

public partial class PhotoTerminal : Node3D, IInteractable
{
    Control GradingUI = null!;
    Button ReturnBtn = null!;

    public override void _Ready()
    {
        GradingUI = GetNode<Control>("GradingUI");
        ReturnBtn = GradingUI.GetNode<Button>("ReturnBtn");
        ReturnBtn.Pressed += () => ToggleUI(false);
        ToggleUI(false);
    }

    void IInteractable.OnInteraction()
    {
        ToggleUI(true);
    }

    private void ToggleUI(bool state)
    {
        Dictionary<bool, Input.MouseModeEnum> mouseState = new()
        {
            { true, Input.MouseModeEnum.Visible },
            { false, Input.MouseModeEnum.Captured },
        };

        GradingUI.Visible = state;
        Input.SetMouseMode(mouseState[state]);
    }
}
