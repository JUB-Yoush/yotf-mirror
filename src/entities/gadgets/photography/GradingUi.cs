using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

public partial class GradingUi : Control
{
    PhotoTerminal photoTerminal = null!;
    private LabelSettings styleLabelSettings = GD.Load<LabelSettings>("uid://d1hk046eb0hlq");
    private Button ReturnBtn = null!;
    private TextureRect photoRect = null!;
    private VBoxContainer styleLabels = null!;
    private List<Photo> UploadedPhotos = [];
    private HashSet<Photo> viewedPhotos = [];
    private Label PhotoTotalLabel = null!;
    private Label GalleryTotalLabel = null!;
    private Button PrevBtn = null!;
    private Button NextBtn = null!;
    private int currentPhotoIndex = 0;
    private int GalleryTotal = 0;

    public GradingUi Init(List<Photo> photos, PhotoTerminal photoTerminal)
    {
        this.UploadedPhotos = photos;
        this.photoTerminal = photoTerminal;
        return this;
    }

    public override void _Ready()
    {
        PhotoTotalLabel = GetNode<Label>("PhotoTotalLabel");
        GalleryTotalLabel = GetNode<Label>("GalleryTotalLabel");
        ReturnBtn = GetNode<Button>("ReturnBtn");
        photoRect = GetNode<TextureRect>("PhotoRect");
        styleLabels = GetNode<VBoxContainer>("StyleLabels");
        styleLabels.RemoveAllChildren();
        PrevBtn = GetNode<Button>("PrevBtn");
        NextBtn = GetNode<Button>("NextBtn");

        PrevBtn.Pressed += () =>
        {
            currentPhotoIndex = Math.Max(0, currentPhotoIndex - 1);
            RenderPhotoGrade(currentPhotoIndex);
        };

        NextBtn.Pressed += () =>
        {
            currentPhotoIndex = Math.Min(currentPhotoIndex + 1, UploadedPhotos.Count - 1);
            RenderPhotoGrade(currentPhotoIndex);
        };
        ReturnBtn.Pressed += CloseShop;

        var player = GetTree().CurrentScene.GetNode<PlayerController>("Player");
        player.IsInMenu = true;
        Input.SetMouseMode(Input.MouseModeEnum.Visible);
        RenderPhotoGrade(0);
    }

    private void CloseShop()
    {
        photoTerminal.inShop = false;
        var player = GetTree().CurrentScene.GetNode<PlayerController>("Player");
        player.IsInMenu = false;
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
        QueueFree();
    }

    private void RenderPhotoGrade(int index)
    {
        if (UploadedPhotos.Count == 0)
        {
            MakeStyleLabel("None", "Bro there's nothing in this one.", 0);
            return;
        }

        styleLabels.RemoveAllChildren();

        var photo = UploadedPhotos[index];
        photoRect.Texture = photo.Data.ToTexture();
        if (photo.SubjectGrades.Count == 0)
        {
            MakeStyleLabel("None", "Bro there's nothing in this one.", 0);
            return;
        }

        int sum = 0;
        foreach (var (subject, grade) in photo.SubjectGrades)
        {
            MakeStyleLabel(subject, "Facing Score", Math.Floor(grade.FacingScore * 100.0));
            MakeStyleLabel(subject, "Centered Score", Math.Floor(grade.CenterScore * 100.0));
            MakeStyleLabel(subject, "Size Score", Math.Floor(grade.SizeScore * 100.0));
            sum += ((int)((grade.FacingScore + grade.CenterScore + grade.SizeScore) * 100));
        }
        PhotoTotalLabel.Text = $"TOTAL: {sum}";
        if (!viewedPhotos.Contains(photo))
        {
            GalleryTotal += sum;
            viewedPhotos.Add(photo);
        }
        GalleryTotalLabel.Text = $"Gallery Total: {GalleryTotal}";
        //photoTerminal.LabelText = $"{GalleryTotal:D6}";
        var player = GetTree().CurrentScene.GetNode<PlayerController>("Player");
        var stats = player.GetNode<PlayerStats>("Stats");
        stats.Money += GalleryTotal;
    }

    private void MakeStyleLabel(string subject, string desc, double score)
    {
        var label = new Label
        {
            LabelSettings = styleLabelSettings,
            Text = $"{subject}: {desc} ({score})",
        };
        styleLabels.AddChild(label);
    }
}
