using System;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Player : CharacterBody3D, ITakeDamage
{
    public override void _Notification(int what) => this.Notify(what);

    public static Action<IPlayerState, IPlayerState>? StateChanged;

    // ====================== REFERENCES ======================
    [Node]
    public required Node3D Skin { set; get; }

    public Vec3 SkinRestPosition;

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

    public Vec3 CollisionPivot;

    bool renderingAlert = false;

    public bool InNegationArea = false;
    public int WaterVolumeCount = 0; // for handling overlapping water volumes

    [Export]
    Lab? StartLab = null;

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
    [Export(PropertyHint.Range, "1,50")]
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
        get =>
            (Lab.Map.TryGetValue(0, out var lab) == false)
                ? 0f
                : (lab.GlobalPosition.Y - GlobalPosition.Y);
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
                tween.AnimateProperty(Arm, SpringArm3D.PropertyName.SpringLength, 0.0f, 0.33f);
                tween.Fn(() => Skin.Visible = false);
            }
            else
            {
                if (Arm == null)
                    return;
                Skin.Visible = true;
                CreateTween()
                    .AnimateProperty(Arm, SpringArm3D.PropertyName.SpringLength, 2.0f, 0.33f);
            }
        }
    }

    private bool collisionEnabled = true;

    public Tween? shockTween = null;

    bool gettingShocked = false;

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

    float ITakeDamage.Health
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
    bool ITakeDamage.Invincible
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
    bool ITakeDamage.IsDead
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public readonly WalkingState WalkingState = new();
    public readonly SwimmingState SwimmingState = new();
    public readonly NoClipState NoClipState = new();

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

        if (StartLab != null)
        {
            GlobalPosition = StartLab.PlayerSpawn.GlobalPosition;
            Lab.CurrentLab = StartLab;
        }
    }

#if DEBUG
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("noclip_on"))
            SetState(NoClipState);
        if (Input.IsActionJustPressed("noclip_off"))
            SetState(WalkingState);
    }
#endif

    public override void _PhysicsProcess(double delta)
    {
        //Log.PrintLn(Camera.Fov);
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

    internal void UpdateBodyWalkDirection(Vec3 direction, float delta)
    {
        if (direction == Vec3.Zero)
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

    internal void UpdateBodySwimRotation(Vec3 rotation)
    {
        Basis rotBasis = Basis.FromEuler(rotation);

        CollisionShapeBody.Rotation = rotation;

        Skin.Rotation = rotation; // TODO(j): lerp
        Skin.Position = CollisionPivot + rotBasis * (SkinRestPosition - CollisionPivot);
    }

    internal Vec3 GetCameraRelativeDirection()
    {
        Vec3 inputDir = Vec3.Zero;

        inputDir -= Camera.GlobalTransform.Basis.X * Input.GetActionStrength("left");
        inputDir += Camera.GlobalTransform.Basis.X * Input.GetActionStrength("right");
        inputDir -= Camera.GlobalTransform.Basis.Z * Input.GetActionStrength("up");
        inputDir += Camera.GlobalTransform.Basis.Z * Input.GetActionStrength("down");

        return inputDir;
    }

    internal void GetShocked()
    {
        if (gettingShocked)
            return;
        gettingShocked = true;
        Audio.PlaySfx(Sfx.PowerDown);

        for (int i = 0; i < Inventory.Capacity; i++)
        {
            var item = Inventory.Items[i];
            if (item != null && item.IsValid() && item is IDroppable droppable)
            {
                Log.PrintLn($"{item.Name}");
                var dropItem = IDroppable.MakeDropItem(droppable);
                var randomDir = new Vec3(GD.Randf(), 0.5f, GD.Randf()).Normalized();
                GetTree().CurrentScene.AddChild(dropItem);
                dropItem.GlobalTransform = GlobalTransform;
                dropItem.stock = item.stock;
                if (item is Disposable dispose)
                {
                    dropItem.restore = dispose.restore;
                    dropItem.RestoreAmount = dispose.restoreAmount;
                }
                dropItem.ApplyImpulse(randomDir * 5);
                Inventory.RemoveItem(i);
            }
        }

        shockTween = CreateTween();
        //disable HUD
        shockTween.AnimateProperty(HUD, Control.PropertyName.Modulate, new Color(0xffffff00), .5f);
        shockTween.Fn(() =>
        {
            Alert.Visible = true;
            Alert.RenderGradually(
                "WARNING: SURGE PROTECTION ACTIVATED.\nRESETING POWER SUPPLY...",
                0.02f
            );
        });
        shockTween.TweenInterval(4f);
        shockTween.AnimateProperty(HUD, Control.PropertyName.Modulate, new Color(0xffffffff), .5f);
        shockTween.AnimateProperty(
            Alert,
            Control.PropertyName.Modulate,
            new Color(0xffffff00),
            .5f,
            true
        );
        shockTween.Fn(() => Alert.Visible = false);
        shockTween.Fn(() =>
        {
            Audio.PlaySfx(Sfx.PowerUp);
            HUD.Visible = true;
        });
        shockTween.AnimateProperty(
            Alert,
            Control.PropertyName.Modulate,
            new Color(0xffffffff),
            1f,
            true
        );
        shockTween.Finished += () =>
        {
            shockTween = null;
            gettingShocked = false;
        };
    }

    void ITakeDamage.TakeDamage(float amount, Vec3 knockback, Node3D source)
    {
        //hit
        Stats.Oxygen -= amount;
        Velocity += knockback;
        Audio.PlaySfx(Sfx.Hit);
        if (source is Eel)
        {
            GetShocked();
        }
        var ScreenFlash = HUD.ScreenColor;
        ScreenFlash.Visible = true;
        var tween = CreateTween();
        tween.AnimateProperty(
            ScreenFlash,
            ColorRect.PropertyName.Color,
            new Color(1, 0, 0, 1),
            .2f
        );

        tween.AnimateProperty(
            ScreenFlash,
            ColorRect.PropertyName.Color,
            new Color(0, 0, 0, 0),
            .2f
        );
    }

    internal void MakeAlert(string str)
    {
        if (renderingAlert == true)
        {
            GD.PushWarning("Alert already rendering");
            return;
        }

        Alert.Visible = true;
        Alert.RenderGradually(str);
        Audio.PlaySfx(Sfx.Alert);
        CreateTween()
            .Fn(
                () =>
                {
                    Alert.Visible = false;
                    renderingAlert = false;
                },
                2
            );
    }
}
