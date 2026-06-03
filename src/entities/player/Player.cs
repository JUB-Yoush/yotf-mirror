using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Player : CharacterBody3D
{
    public override void _Notification(int what) => this.Notify(what);

    public static Action<IPlayerState, IPlayerState>? StateChanged;

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

    [Node]
    public required Hud HUD { set; get; }

    [Node]
    public required ColorRect FishEyeRect { set; get; }

    [Node]
    public required ColorRect UnderwaterRect { set; get; }

    [Node]
    public required PlayerStats Stats { set; get; }

    [Node]
    public required Inventory Inventory { set; get; }

    [Node]
    public required Label Alert { set; get; }

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
    public float SwimSpeed = 5.0f;

    [Export]
    public float SwimRotationSpeed = 5.0f;

    [Export]
    public float SwimDamping = 2.0f;

    [Export]
    public float SwimBoostMultiplier = 2.5f;

    // ====================== DEBUG CONFIG ======================
    [ExportCategory("Debug")]
    private bool firstPerson = false;

    //TODO(j) this needs to be normalized based on the size of the map. or somthing.
    public float Depth
    {
        set;
        get => (Lab.CurrentLab.GlobalPosition.Y - GlobalPosition.Y);
    }

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
                tween.LerpProperty(Arm, SpringArm3D.PropertyName.SpringLength, 0.0f, 0.33f);
                tween.Fn(() => Skin.Visible = false);
            }
            else
            {
                if (Arm == null)
                    return;
                Skin.Visible = true;
                CreateTween().LerpProperty(Arm, SpringArm3D.PropertyName.SpringLength, 2.0f, 0.33f);
            }
        }
    }

    private bool collisionEnabled = true;

    public Tween? shockTween = null;

    //TODO(j) should probably be an enum for player interaction state
    public bool IsInMenu
    {
        get;
        set
        {
            field = value;
            this.GetNode<CameraManager>().GetNode<Camera3D>()!.Visible = !field;
            HUD.Visible = !field;
            FishEyeRect.Visible = !field;
        }
    }

    public bool IsLookingInCamera
    {
        get;
        set
        {
            field = value;
            this.GetNode<CameraManager>().GetNode<Camera3D>()!.Visible = !field;
            HUD.Visible = !field;
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
        Log.PrintLn("player ready");
        SkinRestPosition = Skin.Position;

        CollisionPivot = CollisionShapeBody.Position;

        ProceduralAnimator ??= GetNode<Node3D>("Skin").GetNode<ProceduralAnimator>()!;
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
        StateChanged?.Invoke(CurrentState, newState);
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

        Skin.Rotation = rotation; // TODO(j): lerp
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

    internal void GetShocked(Vector3 ShockSource)
    {
        if (shockTween != null)
            return;

        //hit
        Stats.Injuries += 3;
        //kb
        var dir = (GlobalPosition - ShockSource).Normalized();
        Velocity += dir * 15;

        // drop ur stuff
        for (int i = 0; i < Inventory.Capacity; i++)
        {
            var item = Inventory.Items[i];
            if (item != null && item is IDroppable droppable)
            {
                var dropItem = IDroppable.MakeDropItem(droppable);
                dropItem.GlobalTransform = GlobalTransform;
                GetTree().CurrentScene.AddChild(dropItem);
                Inventory.RemoveItem(i);
            }
        }
        shockTween = CreateTween();
        //disable HUD
        shockTween.LerpProperty(HUD, Control.PropertyName.Modulate, new Color(0xffffff00), .5f);
        shockTween.Fn(() =>
        {
            Alert.Visible = true;
            Alert.RenderGradually(
                "WARNING: SURGE PROTECTION ACTIVATED.\nRESETING POWER SUPPLY...",
                0.02f
            );
        });
        shockTween.TweenInterval(4f);
        shockTween.LerpProperty(HUD, Control.PropertyName.Modulate, new Color(0xffffffff), .5f);
        shockTween.LerpProperty(
            Alert,
            Control.PropertyName.Modulate,
            new Color(0xffffff00),
            .5f,
            true
        );
        shockTween.Fn(() => Alert.Visible = false);
        shockTween.Fn(() => HUD.Visible = true);
        shockTween.LerpProperty(
            Alert,
            Control.PropertyName.Modulate,
            new Color(0xffffffff),
            .5f,
            true
        );
        shockTween.Finished += () => shockTween = null;
    }
}
