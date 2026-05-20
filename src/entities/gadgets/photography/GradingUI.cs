using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class GradingUI : Control
{
    private static readonly PackedScene Packed = GD.Load<PackedScene>("uid://b627ai4x06ylo");

    private static readonly Dictionary<string, int> maxPhotoScores = [];

    public override void _Notification(int what) => this.Notify(what);

    public static GradingUI New(List<Photo> photos, PhotoTerminal photoTerminal)
    {
        var gradeUI = Packed.Instantiate<GradingUI>();
        gradeUI.uploadedPhotos = photos;
        gradeUI.photoTerminal = photoTerminal;
        return gradeUI;
    }

    private LabelSettings styleLabelSettings = GD.Load<LabelSettings>("uid://d1hk046eb0hlq");

    [Node]
    public required Button ReturnBtn { set; get; }

    [Node]
    public required TextureRect PhotoRect { set; get; }

    [Node]
    public required VBoxContainer StyleLabels { set; get; }

    [Node]
    public required Label PhotoTotalLabel { set; get; }

    [Node]
    public required Label GalleryTotalLabel { set; get; }

    [Node]
    public required Button PrevBtn { set; get; }

    [Node]
    public required Button NextBtn { set; get; }

    PhotoTerminal photoTerminal = null!;
    private int currentPhotoIndex = 0;
    private int GalleryTotal = 0;
    private List<Photo> uploadedPhotos = [];
    private readonly HashSet<Photo> viewedPhotos = [];

    public override void _Ready()
    {
        StyleLabels.RemoveAllChildren();
        PrevBtn.Pressed += () =>
        {
            currentPhotoIndex = Math.Max(0, currentPhotoIndex - 1);
            RenderPhotoGrade(currentPhotoIndex);
        };

        NextBtn.Pressed += () =>
        {
            currentPhotoIndex = Math.Min(currentPhotoIndex + 1, uploadedPhotos.Count - 1);
            RenderPhotoGrade(currentPhotoIndex);
        };
        ReturnBtn.Pressed += CloseShop;

        var player = this.SceneRoot().GetNode<PlayerController>()!;
        player.IsInMenu = true;
        Input.SetMouseMode(Input.MouseModeEnum.Visible);
        RenderPhotoGrade(0);
    }

    private void CloseShop()
    {
        photoTerminal.inShop = false;
        var player = this.SceneRoot().GetNode<PlayerController>()!;
        player.IsInMenu = false;
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
        QueueFree();
    }

    private void RenderPhotoGrade(int index)
    {
        if (uploadedPhotos.Count == 0)
        {
            MakeStyleLabel("None", "Bro there's nothing in this one.", 0);
            return;
        }

        StyleLabels.RemoveAllChildren();

        var photo = uploadedPhotos[index];
        PhotoRect.Texture = photo.Data.ToTexture();
        if (photo.SubjectGrades.Count == 0)
        {
            MakeStyleLabel("None", "Bro there's nothing in this one.", 0);
            return;
        }

        int sum = 0;
        HashSet<string> newRecords = [];
        foreach (var (subject, grade) in photo.SubjectGrades)
        {
            MakeStyleLabel(subject, "Facing Score", Math.Floor(grade.FacingScore * 100.0));
            MakeStyleLabel(subject, "Centered Score", Math.Floor(grade.CenterScore * 100.0));
            MakeStyleLabel(subject, "Size Score", Math.Floor(grade.SizeScore * 100.0));

            // record highest scoring photo taken of this subject
            if (!maxPhotoScores.TryGetValue(subject, out var highestScore) || highestScore < sum)
            {
                MakeStyleLabel(subject, "New Record!", 0);
                sum +=
                    ((int)((grade.FacingScore + grade.CenterScore + grade.SizeScore) * 100))
                    - highestScore;
                maxPhotoScores.TryAdd(subject, sum);
                newRecords.Add(subject);
            }
            else
            {
                MakeStyleLabel(subject, "More Valuable Photo already taken...", 0);
            }
        }
        PhotoTotalLabel.Text = $"TOTAL: {sum}";
        if (!viewedPhotos.Contains(photo))
        {
            GalleryTotal += sum;
            viewedPhotos.Add(photo);
        }
        GalleryTotalLabel.Text = $"Gallery Total: {GalleryTotal}";
        var player = this.SceneRoot().GetNode<PlayerController>()!;
        var stats = player.GetNode<PlayerStats>()!;
        stats.Money += GalleryTotal;
        stats.TotalGalleryScore += GalleryTotal;
    }

    private void MakeStyleLabel(string subject, string desc, double score)
    {
        var label = new Label
        {
            LabelSettings = styleLabelSettings,
            Text = $"{subject}: {desc} ({score})",
        };
        StyleLabels.AddChild(label);
    }
}
