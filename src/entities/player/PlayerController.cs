using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class PlayerController : CharacterBody3D
{
    public override void _Notification(int what) => this.Notify(what);

    // ====================== REFERENCES ======================
    [Node]
    public required Node3D Skin { set; get; }

    public Vector3 SkinRestPosition;

    [Node]
    public required Camera3D Camera { set; get; }

    [Node]
    public required CollisionShape3D CollisionShapeBody { set; get; }

    [Node]
    public required SpringArm3D Arm { set; get; }

    public Vector3 CollisionPivot;

    // ====================== MOVEMENT CONFIG ======================
    [ExportCategory("Land Movement")]
    [Export(PropertyHint.Range, "1,50")]
    public float MoveSpeed = 3.0f;

    [Export]
    public float JumpSpeed = 7.0f;

    [Export]
    public float Weight = 2.0f;

    [Export]
    public float RotationSpeed = 10.0f;

    internal float Gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    [ExportCategory("Swim Movement")]
    [Export(PropertyHint.Range, "5,50")]
    public float SwimSpeed = 10.0f;

    [Export]
    public float SwimRotationSpeed = 5.0f;

    [Export]
    public float SwimDamping = 2.0f;

    [Export]
    public float SwimBoostMultiplier = 2.5f;

    // ====================== DEBUG CONFIG ======================
    [ExportCategory("Debug")]
    private bool firstPerson = false;

    [Export]
    public bool FirstPerson
    {
        get => firstPerson;
        set
        {
            firstPerson = value;
            if (firstPerson)
            {
                if (Arm == null)
                    return;
                Tween tween = CreateTween();
                tween.TweenProperty(Arm, "spring_length", 0.0f, 0.33);
                tween.Fn(() => Skin.Visible = false);
            }
            else
            {
                if (Arm == null)
                    return;
                Skin.Visible = true;
                CreateTween().TweenProperty(Arm, "spring_length", 2.0f, 0.33);
            }
        }
    }

    private bool collisionEnabled = true;

    public bool IsInMenu
    {
        get;
        set
        {
            field = value;
            this.GetNode<CameraManager>().GetNode<Camera3D>()!.Visible = !field;
        }
    }

    [Export]
    public bool CollisionEnabled
    {
        get => collisionEnabled;
        set
        {
            collisionEnabled = value;
            GetNode<CollisionShape3D>("CollisionShapeBody").Disabled = !collisionEnabled;
            GetNode<CollisionShape3D>("CollisionShapeRay").Disabled = !collisionEnabled;
        }
    }

    // ====================== INTERNAL STATE ======================
    [Export]
    public ProceduralAnimator ProceduralAnimator = null!;

    public IPlayerState CurrentState { get; private set; } = null!;
    public PlayerState State => CurrentState.Type;
    public float YawVelocity { get; private set; }

    public readonly WalkingState WalkingState = new();
    public readonly SwimmingState SwimmingState = new();

    private RayCast3D raycast = null!;

    public override void _EnterTree()
    {
        if (int.TryParse(Name, out int peerId))
        {
            SetMultiplayerAuthority(peerId);
            if (Multiplayer.MultiplayerPeer != null)
                ProcessMode = IsMultiplayerAuthority()
                    ? ProcessModeEnum.Inherit
                    : ProcessModeEnum.Disabled;
        }
    }

    public override void _Ready()
    {
        // raycast = GetNode<RayCast3D>("CameraManager/Camera3D/RayCast3D");
        // Camera ??= GetNode<Camera3D>("%Camera3D");

        // Skin ??= GetNode<Node3D>("SkrunkoSkin");

        Log.PrintLn("player ready");
        SkinRestPosition = Skin.Position;

        // CollisionShapeBody ??= GetNode<CollisionShape3D>("CollisionShapeBody");
        CollisionPivot = CollisionShapeBody.Position;

        // ProceduralAnimator ??= GetNode<ProceduralAnimator>("ProceduralAnimator");

        // springArm = GetNode<SpringArm3D>("CameraManager/Arm");

        if (IsMultiplayerAuthority())
        {
            Camera.Current = true;
        }

        CurrentState = WalkingState;
        WalkingState.Enter(this);
        FirstPerson = true;
    }

#if DEBUG
    public override void _Process(double delta)
    {
        if (Input.IsKeyPressed(Key.KpAdd) || Input.IsKeyPressed(Key.Equal))
            MoveSpeed = Mathf.Clamp(MoveSpeed + 0.5f, 5, 9999);
        if (Input.IsKeyPressed(Key.KpSubtract) || Input.IsKeyPressed(Key.Minus))
            MoveSpeed = Mathf.Clamp(MoveSpeed - 0.5f, 5, 9999);
    }
#endif

    public override void _PhysicsProcess(double delta)
    {
        CurrentState.Update(this, (float)delta);
        // check for items in the player raycast
    }

    internal void SetState(IPlayerState newState)
    {
        if (CurrentState == newState)
            return;
        CurrentState?.Exit(this);
        CurrentState = newState;
        CurrentState.Enter(this);
        ProceduralAnimator.OnStateChanged(newState.Type);
    }

    internal void UpdateBodyDirection(Vector3 direction, float delta)
    {
        if (direction == Vector3.Zero)
        {
            YawVelocity = 0f;
            return;
        }
        float targetAngle = Mathf.Atan2(direction.X, direction.Z);
        float prevYaw = Skin.Rotation.Y;
        Skin.Rotation = Skin.Rotation with
        {
            Y = Mathf.LerpAngle(prevYaw, targetAngle, RotationSpeed * delta),
        };

        // update yaw velocity for animation purposes
        YawVelocity = Mathf.AngleDifference(prevYaw, Skin.Rotation.Y) / delta;
    }

    internal void UpdateBodyRotation(Vector3 rotation)
    {
        Basis rotBasis = Basis.FromEuler(rotation);

        CollisionShapeBody.Rotation = rotation;

        Skin.Rotation = rotation; // TODO: lerp
        Skin.Position = CollisionPivot + rotBasis * (SkinRestPosition - CollisionPivot);
    }

    internal Vector3 GetCameraRelativeDirection()
    {
        Vector3 inputDir = Vector3.Zero;

        inputDir -= Camera.GlobalTransform.Basis.X * Input.GetActionStrength("left");
        inputDir += Camera.GlobalTransform.Basis.X * Input.GetActionStrength("right");
        inputDir -= Camera.GlobalTransform.Basis.Z * Input.GetActionStrength("up");
        inputDir += Camera.GlobalTransform.Basis.Z * Input.GetActionStrength("down");

        return inputDir;
    }
}
