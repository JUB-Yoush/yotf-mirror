using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

public partial class PhotoTerminal : Node3D, IInteractable
{
    private Mesh mesh = null!;
    public bool inShop = false;

    private static readonly PackedScene GradingUI = GD.Load<PackedScene>("uid://b627ai4x06ylo");

    public override void _Ready()
    {
        mesh = GetNode<MeshInstance3D>("MeshInstance3D").Mesh;
    }

    void IInteractable.OnInteraction()
    {
        if (inShop)
            return;
        inShop = true;

        var player = GetTree().CurrentScene.GetNode<PlayerController>("Player");
        var inventory = player.GetNode<Inventory>("Inventory");

        if (inventory.GetItemIndex("camera") == -1)
            return;

        var cam = inventory.GetNode<PhotoCamera>(inventory.GetItemIndex("camera").ToString());
        var gradeUi = GradingUI.Instantiate<GradingUi>().Init(cam.Photos, this);
        cam.ClearPhotos();
        GetTree().CurrentScene.AddChild(gradeUi);
    }

    public Mesh GetMesh() => mesh;
}
