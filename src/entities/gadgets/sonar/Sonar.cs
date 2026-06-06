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

    [Export]
    float range = 30f;

    [Export]
    float exponentFalloff = 1.5f;

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

    ISonarable? closest = null!;
    Label Label = null!;

    public override void _Ready()
    {
        player = GetParent().GetParent<Player>();
        playerCamera = player.GetNode<CameraManager>().GetNode<Camera3D>()!;
        Label = player.HUD.SonarLabel;

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
        Label.Visible = true;
        player.Alert.RenderGradually("LOCATING...", 0.02f);
    }

    public override void Unequipped()
    {
        Visible = false;
        Reticles.Visible = false;
        player.Alert.Visible = false;
        Label.Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (!CurrentItem)
            return;

        if (@event.IsActionPressed("drop_item"))
        {
            var dropItem = IDroppable.MakeDropItem(this);
            dropItem.GlobalTransform = playerCamera.GlobalTransform;
            GetTree().CurrentScene.AddChild(dropItem);
            player.Inventory.RemoveCurrentItem();
        }
    }

    public override void _Process(double delta)
    {
        if (!CurrentItem)
            return;

        foreach (var sonarable in sonarItems)
        {
            var dist = (sonarable.GlobalPosition - playerCamera.GlobalPosition).Length();
            UpdateRetacleUI(sonarable);
            closest ??= sonarable;
            var newdot = GetDotToTarget(sonarable);
            var highestDot = GetDotToTarget(closest);
            if (newdot >= highestDot)
            {
                closest = sonarable;
            }

            if (!sonarable.Discovered)
                sonarable.Discovered = (dist <= sonarable.DiscoveryDistance);
        }
        if (closest == null)
            return;

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
        var reticle = ReticleMap[sonarable];
        var half = reticle.GetNodes<TextureRect>();
        var dist = (sonarable.GlobalPosition - playerCamera.GlobalPosition).Length();

        if (dist > range)
        {
            half[0].Modulate = new(0, 0, 0, 0);
            half[1].Modulate = new(0, 0, 0, 0);
            return;
        }
        var opacityScale = Mathf.Pow((Math.Clamp((range - dist) / range, 0, 1)), exponentFalloff);

        var dot = GetDotToTarget(sonarable);
        var screen = GetViewport().GetVisibleRect().Size;
        var colorScale = dot;
        half[0].Modulate = new(1 - colorScale, colorScale, 0, opacityScale);
        half[1].Modulate = new(1 - colorScale, colorScale, 0, opacityScale);
        half[0].GlobalPosition = new((((screen.X / 2) - 64) * dot), (screen.Y / 2) - 64);
        half[1].GlobalPosition = new(screen.X - ((screen.X / 2) * dot), (screen.Y / 2) - 64);
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
