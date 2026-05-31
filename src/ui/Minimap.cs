using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Minimap : Control
{
    public override void _Notification(int what) => this.Notify(what);

    [Node]
    public required ColorRect MiniMapRect { set; get; }

    [Export]
    float miniMapZoom = 5f;

    Vector2 origin;

    private readonly List<IOnMiniMap> miniMapItems = [];
    private readonly Dictionary<IOnMiniMap, ColorRect> MapIcons = [];
    private PlayerController player = null!;

    public override void _Ready()
    {
        player = this.SceneRoot().GetNode<PlayerController>()!;
        origin = MiniMapRect.Size / 2;
        GetTree().NodeAdded += (node) =>
        {
            if (node is IOnMiniMap mapItem)
                AddMapItem(mapItem);
        };

        GetTree().NodeRemoved += (node) =>
        {
            if (node is IOnMiniMap mapItem)
                RemoveMapItem(mapItem);
        };

        foreach (var mapItem in this.SceneRoot().GetNodes<IOnMiniMap>(true))
        {
            AddMapItem(mapItem);
        }
    }

    void AddMapItem(IOnMiniMap item)
    {
        miniMapItems.Add(item);
        var colorIcon = new ColorRect { Size = new(32, 32), Color = new(.5f, .5f, .5f) };
        MiniMapRect.AddChild(colorIcon);
        MapIcons.Add(item, colorIcon);
    }

    void RemoveMapItem(IOnMiniMap item)
    {
        miniMapItems.Remove(item);
        MapIcons[item].QueueFree();
        MapIcons.Remove(item);
    }

    public override void _Process(double delta)
    {
        foreach (var (item, rect) in MapIcons)
        {
            // player direction
            var forwardXZ = -player.Camera.GlobalBasis.Z.XZ();
            // rotate so up on map is player facing
            var rotation = -forwardXZ.Angle() - Mathf.Pi / 2;
            // get position relative to player
            var relativePos = item.RelativePositon(player.GlobalPosition.XZ());
            // rotate to minimap angle
            var rotated = relativePos.Rotated(rotation);
            rect.Position = origin + (rotated * miniMapZoom) - rect.Size / 2;
        }
    }
}
