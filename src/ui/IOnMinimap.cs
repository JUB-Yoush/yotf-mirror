using System;
using Godot;

namespace Yotf;

// only used by classes that are based on Node3D, so we can cast to that safely
public interface IOnMiniMap
{
    virtual bool CanBeOnMinimap() => true;

    //vector pointing from the node to the player position
    Vector2 RelativePositon(Vector2 playerXZ) => ((Node3D)this).GlobalPosition.XZ() - playerXZ;
}
