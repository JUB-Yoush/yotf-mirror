using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class PhotoTerminal : Node3D, IInteractable
{
    public override void _Notification(int what) => this.Notify(what);

    [Node]
    public required MeshInstance3D Mesh { set; get; }

    [Node]
    public required Label3D ScoreLabel { set; get; }

    public bool inShop = false;

    public int TotalGalleryScore = 0;
    public string LabelText
    {
        get { return ScoreLabel?.Text; }
        set { ScoreLabel?.Text = value; }
    }

    public required Mesh InteractionMesh
    {
        get => Mesh.Mesh;
        set;
    }

    void IInteractable.OnInteraction()
    {
        if (inShop)
            return;
        inShop = true;

        var player = GetTree().CurrentScene.GetNode<Player>("Player");
        var inventory = player.GetNode<Inventory>("Inventory");

        if (inventory.GetItemIndex("Camera") == -1)
            return;

        var cam = inventory.GetNode<PhotoCamera>(inventory.GetItemIndex("Camera").ToString());
        var lab = GetParent<Lab>();
        cam.Film = PlayerStats.MaxFilm;
        var gradeUI = GradingUI.New(cam.Photos, this, lab.Index);
        cam.ClearPhotos();
        GetTree().CurrentScene.AddChild(gradeUI);
    }

    public Mesh GetMesh() => Mesh.Mesh;
}
