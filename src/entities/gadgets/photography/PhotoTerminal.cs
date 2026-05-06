using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

public partial class PhotoTerminal : Node3D, IInteractable
{
    Control GradingUI = null!;
    Button ReturnBtn = null!;
    TextureRect photoRect = null!;
    List<Photo> UploadedPhotos = [];

    public override void _Ready()
    {
        GradingUI = GetNode<Control>("GradingUI");
        ReturnBtn = GradingUI.GetNode<Button>("ReturnBtn");
        photoRect = GradingUI.GetNode<TextureRect>("PhotoRect");
        ReturnBtn.Pressed += () => ToggleUI(false);
        ToggleUI(false);
    }

    void IInteractable.OnInteraction()
    {
        ToggleUI(true);
        var inventory = GetTree()
            .CurrentScene.GetNode<PlayerController>("Player")
            .GetNode<Inventory>("Inventory");
        if (inventory.GetItemIndex("camera") != -1)
        {
            UploadedPhotos = inventory
                .GetNode<PhotoCamera>(inventory.GetItemIndex("camera").ToString())
                .Photos;
        }
        photoRect.Texture = UploadedPhotos[^1].Data.ToTexture();
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
