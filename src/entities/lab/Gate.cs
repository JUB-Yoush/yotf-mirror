using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Gate : StaticBody3D, IInteractable
{
    public override void _Notification(int what) => this.Notify(what);

    [Node]
    public required Label3D GateLabel { set; get; }

    [Node]
    public required MeshInstance3D Mesh { set; get; }

    public Lab lab = null!;

    public required Mesh InteractionMesh
    {
        get => Mesh.Mesh;
        set;
    }

    bool canOpen;

    public override void _Ready()
    {
        lab = GetParent<Lab>();
        var playerstats = this.SceneRoot().GetNode<PlayerController>().GetNode<PlayerStats>()!;
        playerstats.GalleryScoreUpdated += GalleryScoreUpdated;
    }

    public override void _ExitTree()
    {
        var playerstats = this.SceneRoot().GetNode<PlayerController>().GetNode<PlayerStats>()!;
        playerstats.GalleryScoreUpdated -= GalleryScoreUpdated;
    }

    public void GalleryScoreUpdated(int score)
    {
        GateLabel.Text = $"{score}/{lab.requiredGalleryScore}";
        canOpen = score >= lab.requiredGalleryScore;
    }

    public void OnInteraction()
    {
        if (canOpen)
        {
            //Lab.SetCurrentLab(lab);
            Lab.CurrentLab = lab;
        }
    }
}
