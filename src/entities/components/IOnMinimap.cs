using System;
using Godot;

namespace Yotf;

/// <summary>
/// Allows Node3Ds to be visible on the minimap.
/// </summary>
public interface IOnMiniMap
{
    virtual bool CanBeOnMinimap() => true;

    //public bool VisibleOnMap { set; get; }

    Vec2 RelativePositon(Vec2 playerXZ) => ((Node3D)this).GlobalPosition.XZ() - playerXZ;
}
