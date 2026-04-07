using Godot;

public partial class PlayerController : CharacterBody3D
{
    // ====================== REFERENCES ======================
    [ExportCategory("References")]
    [Export]
    internal Node3D _skin;
    internal Vector3 _skinRestPosition;
    [Export]
    private Camera3D _camera;
    [Export]
    private CollisionShape3D _collisionShapeBody;
    internal Vector3 _collisionPivot;

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

    internal float _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    [ExportCategory("Swim Movement")]
    [Export(PropertyHint.Range, "5,50")]
    public float SwimSpeed = 10.0f;

    [Export]
    public float SwimRotationSpeed = 5.0f;

    [Export]
    public float SwimDamping = 2.0f;

    // ====================== DEBUG CONFIG ======================
    [ExportCategory("Debug")]
    private bool _firstPerson = false;

    [Export]
    public bool FirstPerson
    {
        get => _firstPerson;
        set
        {
            _firstPerson = value;
            if (_firstPerson)
            {
                Tween tween = CreateTween();
                tween.TweenProperty(
                    GetNode<SpringArm3D>("CameraManager/Arm"),
                    "spring_length",
                    0.0f,
                    0.33
                );
                tween.TweenCallback(Callable.From(() => GetNode<Node3D>("Skin").Visible = false));
            }
            else
            {
                GetNode<Node3D>("Skin").Visible = true;
                CreateTween()
                    .TweenProperty(
                        GetNode<SpringArm3D>("CameraManager/Arm"),
                        "spring_length",
                        6.0f,
                        0.33
                    );
            }
        }
    }

    [Export]
    public float FlySpeed = 2.0f;

    private bool _collisionEnabled = true;

    [Export]
    public bool CollisionEnabled
    {
        get => _collisionEnabled;
        set
        {
            _collisionEnabled = value;
            GetNode<CollisionShape3D>("CollisionShapeBody").Disabled = !_collisionEnabled;
            GetNode<CollisionShape3D>("CollisionShapeRay").Disabled = !_collisionEnabled;
        }
    }

    // ====================== INTERNAL STATE ======================
    [Export]
    private ProceduralAnimator _proceduralAnimator;

    private IPlayerState _currentState;
    public PlayerState State => _currentState.Type;
    private float _yawVelocity;
    public float YawVelocity
    {
        get => _yawVelocity;
        private set => _yawVelocity = value;
    }

    private readonly WalkingState _walkingState = new();
    private readonly FlyingState _flyingState = new();
    private readonly SwimmingState _swimmingState = new();

    public override void _Ready()
    {
        _camera ??= GetNode<Camera3D>("%Camera3D");

        _skin ??= GetNode<Node3D>("Skin");
        _skinRestPosition = _skin.Position;

        _collisionShapeBody ??= GetNode<CollisionShape3D>("CollisionShapeBody");
        _collisionPivot = _collisionShapeBody.Position;

        _proceduralAnimator ??= GetNode<ProceduralAnimator>("ProceduralAnimator");

        _currentState = _walkingState;
        _walkingState.Enter(this);
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
        _currentState.Update(this, (float)delta);
    }

    internal void SetState(IPlayerState newState)
    {
        if (_currentState == newState) return;
        _currentState?.Exit(this);
        _currentState = newState;
        _currentState.Enter(this);
        _proceduralAnimator.OnStateChanged(newState.Type);
    }

    internal void UpdateBodyDirection(Vector3 direction, float delta)
    {
        if (direction == Vector3.Zero)
        {
            YawVelocity = 0f;
            return;
        }
        float targetAngle = Mathf.Atan2(direction.X, direction.Z);
        float prevYaw = _skin.Rotation.Y;
        _skin.Rotation = _skin.Rotation with { Y = Mathf.LerpAngle(prevYaw, targetAngle, RotationSpeed * delta) };

        // update yaw velocity for animation purposes
        YawVelocity = Mathf.AngleDifference(prevYaw, _skin.Rotation.Y) / delta;
    }

    internal void UpdateBodyRotation(Vector3 rotation)
    {
        Basis rotBasis = Basis.FromEuler(rotation);

        _collisionShapeBody.Rotation = rotation;

        _skin.Rotation = rotation;
        _skin.Position = _collisionPivot + rotBasis * (_skinRestPosition - _collisionPivot);
    }

    internal Vector3 GetCameraRelativeDirection()
    {
        Vector3 inputDir = Vector3.Zero;

        inputDir -= _camera.GlobalTransform.Basis.X * Input.GetActionStrength("left");
        inputDir += _camera.GlobalTransform.Basis.X * Input.GetActionStrength("right");
        inputDir -= _camera.GlobalTransform.Basis.Z * Input.GetActionStrength("up");
        inputDir += _camera.GlobalTransform.Basis.Z * Input.GetActionStrength("down");

        return inputDir;
    }

#if DEBUG
    public override void _Input(InputEvent pEvent)
    {
        if (pEvent is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            switch (mouseEvent.ButtonIndex)
            {
                case MouseButton.WheelUp:
                    MoveSpeed = Mathf.Clamp(MoveSpeed + 5, 2, 500);
                    break;
                case MouseButton.WheelDown:
                    MoveSpeed = Mathf.Clamp(MoveSpeed - 5, 2, 500);
                    break;
            }
        }
        else if (pEvent is InputEventKey keyEvent && keyEvent.Pressed)
        {
            switch (keyEvent.Keycode)
            {
                case Key.V:
                    FirstPerson = !FirstPerson;
                    break;
                case Key.G:
                    SetState(_currentState == _flyingState ? _walkingState : _flyingState);
                    break;
                case Key.C:
                    CollisionEnabled = !CollisionEnabled;
                    break;
                case Key.F:
                    SetState(_currentState == _swimmingState ? _walkingState : _swimmingState);
                    break;
            }
        }
    }
#endif
}
