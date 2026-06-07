using System;
using System.Collections.Generic;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class GradingUI : Control
{
    private static readonly PackedScene Packed = GD.Load<PackedScene>("uid://b627ai4x06ylo");

    public static readonly Dictionary<string, int> maxPhotoScores = [];
    private static int labLastRanIn = 0;

    private const float PhotoScoreExponent = 2f;

    public override void _Notification(int what) => this.Notify(what);

    public static GradingUI New(List<Photo> photos, PhotoTerminal photoTerminal, int labIndex)
    {
        var gradeUI = Packed.Instantiate<GradingUI>();
        gradeUI.uploadedPhotos = photos;
        gradeUI.photoTerminal = photoTerminal;
        if (labIndex != labLastRanIn)
        {
            maxPhotoScores.Clear();
            labLastRanIn = labIndex;
        }
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

        var player = this.SceneRoot().GetNode<Player>()!;
        player.IsInMenu = true;
        Input.SetMouseMode(Input.MouseModeEnum.Visible);
        //highestScoringPhoto = CalcHighScores();
        RenderPhotoGrade(0);
    }

    private void CloseShop()
    {
        photoTerminal.inShop = false;
        var player = this.SceneRoot().GetNode<Player>()!;
        player.IsInMenu = false;
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
        QueueFree();
    }

    private void RenderPhotoGrade(int index)
    {
        StyleLabels.RemoveAllChildren();

        if (uploadedPhotos.Count == 0 || uploadedPhotos[index].SubjectGrades.Count == 0)
        {
            MakeStyleLabel("None", "Bro there's nothing in this one.", 0);
            return;
        }

        var photo = uploadedPhotos[index];
        PhotoRect.Texture = photo.Data.ToTexture();

        int sum = 0;
        HashSet<string> newRecords = [];
        foreach (var (subject, grade) in photo.SubjectGrades)
        {
            var facingScore = CalculateScoreValue(grade.FacingScore);
            var centeredScore = CalculateScoreValue(grade.CenterScore);
            var sizeScore = CalculateScoreValue(grade.SizeScore);
            var lightScore = CalculateScoreValue(grade.LightScore);
            var total = facingScore + centeredScore + sizeScore + lightScore;

            MakeStyleLabel(subject, "Facing Score", facingScore);
            MakeStyleLabel(subject, "Centered Score", centeredScore);
            MakeStyleLabel(subject, "Size Score", sizeScore);
            MakeStyleLabel(subject, "Light Score", lightScore);

            // record highest scoring photo taken of this subject
            if (!maxPhotoScores.TryGetValue(subject, out var highestScore) || highestScore <= total)
            {
                MakeStyleLabel(subject, "New Highest Scoring!", 0);
                sum += total - highestScore;
                maxPhotoScores.TryAdd(subject, total);
                newRecords.Add(subject);
            }
            else
            {
                MakeStyleLabel(subject, "More Valuable Photo already taken...", 0);
            }
        }
        PhotoTotalLabel.Text = $"TOTAL: {sum}";
        var player = this.SceneRoot().GetNode<Player>()!;
        var stats = player.GetNode<PlayerStats>()!;
        if (viewedPhotos.Add(photo))
        {
            GalleryTotal += sum;
            stats.Money += GalleryTotal;
            stats.TotalGalleryScore += GalleryTotal;
        }
        GalleryTotalLabel.Text = $"Gallery Total: {GalleryTotal}";
    }

    private static int CalculateScoreValue(float score) =>
        (int)Mathf.Floor(Mathf.Pow(score, PhotoScoreExponent) * 100);

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
