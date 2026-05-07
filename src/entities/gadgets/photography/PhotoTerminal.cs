using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

public partial class PhotoTerminal : Node3D, IInteractable
{
    private LabelSettings styleLabelSettings = GD.Load<LabelSettings>("uid://d1hk046eb0hlq");
    private Control GradingUI = null!;
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
    private bool viewingScreen = false;

    public override void _Ready()
    {
        GradingUI = GetNode<Control>("GradingUI");

        PhotoTotalLabel = GradingUI.GetNode<Label>("PhotoTotalLabel");
        GalleryTotalLabel = GradingUI.GetNode<Label>("GalleryTotalLabel");
        ReturnBtn = GradingUI.GetNode<Button>("ReturnBtn");
        photoRect = GradingUI.GetNode<TextureRect>("PhotoRect");
        styleLabels = GradingUI.GetNode<VBoxContainer>("StyleLabels");
        styleLabels.RemoveAllChildren();
        PrevBtn = GradingUI.GetNode<Button>("PrevBtn");
        NextBtn = GradingUI.GetNode<Button>("NextBtn");
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
        ReturnBtn.Pressed += () => ToggleUI(false);
        ToggleUI(false);
    }

    void IInteractable.OnInteraction()
    {
        if (viewingScreen)
            return;

        ToggleUI(true);

        var player = GetTree().CurrentScene.GetNode<PlayerController>("Player");
        var inventory = player.GetNode<Inventory>("Inventory");

        if (inventory.GetItemIndex("camera") != -1)
        {
            var cam = inventory.GetNode<PhotoCamera>(inventory.GetItemIndex("camera").ToString());
            UploadedPhotos.AddRange(cam.Photos);
            cam.ClearPhotos();
        }

        if (UploadedPhotos.Count > 0)
        {
            RenderPhotoGrade(0);
        }
        else
        {
            MakeStyleLabel("None", "Bro there's nothing in this one.", 0);
        }
    }

    private void RenderPhotoGrade(int index)
    {
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

    private void ToggleUI(bool state)
    {
        Dictionary<bool, Input.MouseModeEnum> mouseState = new()
        {
            { true, Input.MouseModeEnum.Visible },
            { false, Input.MouseModeEnum.Captured },
        };

        var player = GetTree().CurrentScene.GetNode<PlayerController>("Player");
        player.IsInMenu = state;
        viewingScreen = state;
        GradingUI.Visible = state;
        Input.SetMouseMode(mouseState[state]);
    }
}
