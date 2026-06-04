using System;
using Godot;

namespace Yotf;

/// <summary>
/// Used to mark "Rooms" within the map. Fish wander behaviour can ensure fish remain in the same room
/// </summary>
[GlobalClass]
public partial class FishRoom : Marker3D;
