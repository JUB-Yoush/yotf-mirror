using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Sonar : Item, IDroppable
{
    public override void _Notification(int what) => this.Notify(what);

    public static PackedScene Packed = GD.Load<PackedScene>(
        "res://src/entities/gadgets/sonar/sonar.tscn"
    );

    public static PackedScene Reticle = GD.Load<PackedScene>(
        "res://src/entities/gadgets/sonar/sonar_reticle.tscn"
    );

    [Export]
    float MinLabelDistance = 10f;

    [Node]
    public required MeshInstance3D Mesh { set; get; }

    [Node]
    public required Control Reticles { set; get; }

    public new PackedScene PackedScene => Packed;

    public new Mesh DropMesh => Mesh.Mesh;

    Camera3D playerCamera = null!;
    Player player = null!;
    List<ISonarable> sonarItems = [];

    readonly Dictionary<ISonarable, Control> ReticleMap = [];

    ISonarable closest = null!;
    Label Label = null!;

    public override void _Ready()
    {
        player = GetParent().GetParent<Player>();
        playerCamera = player.GetNode<CameraManager>().GetNode<Camera3D>()!;
        Label = player.HUD.SonarLabel;
        GetTree().CurrentScene.GetViewport().SizeChanged += UpdateScreenSize;

        GetTree().NodeAdded += (node) =>
        {
            if (node is ISonarable sonarable)
                AddSonarItem(sonarable);
        };

        GetTree().NodeRemoved += (node) =>
        {
            if (node is ISonarable sonarable)
                RemoveSonarItem(sonarable);
        };

        foreach (var sonarable in this.SceneRoot().GetNodes<ISonarable>(true))
        {
            AddSonarItem(sonarable);
        }
    }

    private void UpdateScreenSize()
    {
        throw new NotImplementedException();
    }

    private void AddSonarItem(ISonarable sonarable)
    {
        sonarItems.Add(sonarable);
        var reticle = Reticle.Instantiate<Control>();
        Reticles.AddChild(reticle);
        ReticleMap.Add(sonarable, reticle);
    }

    private void RemoveSonarItem(ISonarable sonarable)
    {
        sonarItems.Remove(sonarable);
        ReticleMap[sonarable].QueueFree();
        ReticleMap.Remove(sonarable);
    }

    public override void Equipped()
    {
        Visible = true;
        player.Alert.Visible = true;
        Reticles.Visible = true;
        player.Alert.RenderGradually("LOCATING...", 0.02f);
    }

    public override void Unequipped()
    {
        Visible = false;
        Reticles.Visible = false;
        player.Alert.Visible = false;
    }

    public override void _Process(double delta)
    {
        if (!CurrentItem)
            return;

        foreach (var sonarable in sonarItems)
        {
            UpdateRetacleUI(sonarable);
            closest ??= sonarable;
            var newdot = GetDotToTarget(sonarable);
            var highestDot = GetDotToTarget(closest);
            if (newdot >= highestDot)
            {
                closest = sonarable;
            }

            var dist = (sonarable.GlobalPosition - playerCamera.GlobalPosition).Length();
            if (!sonarable.Discovered)
                sonarable.Discovered = (dist <= sonarable.DiscoveryDistance);
        }
        var distance = (closest.GlobalPosition - playerCamera.GlobalPosition).Length();
        var distanceText = distance <= MinLabelDistance ? distance.ToString("F1") : "???";
        if (closest.Discovered)
        {
            player.Alert.Text = "DISCOVERED";
        }
        var nameText = closest.Discovered ? closest.Name.ToString() : "UNKNOWN";
        Label.Text = $"{nameText}| {distanceText}m";
    }

    private void UpdateRetacleUI(ISonarable sonarable)
    {
        var dot = GetDotToTarget(sonarable);
        var reticle = ReticleMap[sonarable];
        var half = reticle.GetNodes<Sprite2D>();
        var screen = DisplayServer.WindowGetSize();
        half[0].Modulate = new(dot, dot, dot, 1);
        half[1].Modulate = new(dot, dot, dot, 1);
        half[0].GlobalPosition = new(((screen.X + 128) * dot), screen.Y + 64);
        half[1].GlobalPosition = new(screen.X * 2 - (screen.X - 128) * dot, screen.Y + 64);
    }

    float GetDotToTarget(ISonarable sonarable, bool clamped = true)
    {
        var targetDir = (sonarable.GlobalPosition - playerCamera.GlobalPosition).Normalized();
        var sonarFacingDir = -playerCamera.GlobalTransform.Basis.Z;
        var dot = sonarFacingDir.Dot(targetDir);
        if (clamped)
            return Math.Clamp(dot, 0f, 1f);
        return dot;
    }
}
