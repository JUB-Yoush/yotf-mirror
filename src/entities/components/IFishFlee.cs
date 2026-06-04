using System;

namespace Yotf;

/// <summary>
/// (WIP)
/// Implemented on Fish
/// </summary>
public interface IFishFlee
{
    // last known position of a detected threat so FleeingState can continue fleeing after the threat leaves the detection area
    public Vec3 ThreatPosition { get; internal set; }

    public Node3D? ThreatTarget { get; set; }

    public void SetFleeState();
}
