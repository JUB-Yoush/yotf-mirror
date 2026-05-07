using System;
using Godot;

namespace Yotf;

public partial class PlayerStats : Node
{
    float oxygen;
    float battery;
    float maxOxygen;
    float maxBattery;
    int money;
    int TotalGalleryScore;
    PlayerController player = null!;
}
