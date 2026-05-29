using Godot;

namespace Yotf;

[GlobalClass]
public partial class FishProfile : Resource
{
    [ExportCategory("Movement")]
    [Export]
    public float MoveSpeed = 3f;

    [Export]
    public float RotationSpeed = 4f;

    [Export]
    public float WanderRadius = 6f;

    [ExportCategory("Fleeing")]
    [Export]
    public float FleeSpeed = 7f;

    [Export]
    public float FleeDistance = 12f;

    [Export]
    public float FleeTimeout = 6f;

    [Export(PropertyHint.Range, "0,1")]
    public float NoiseThreshold = 0.4f;

    [ExportCategory("Aggressive")]
    [Export]
    public bool IsAggressive = false;

    // fish gives up the chase when the player exceeds this distance
    [Export]
    public float AggroLeashRadius = 14f;

    [Export]
    public float AttackRange = 1.5f;

    [Export]
    public float AttackCooldown = 1.5f;

    [ExportCategory("Hidden")]
    [Export]
    public bool StartsHidden = false;
}
