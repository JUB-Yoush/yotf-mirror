using System.Collections;
using Godot;

public partial class PlayerController : CharacterBody3D
{
    [ExportCategory("References")]
    [Export]
    private Node3D _skin;
    private Vector3 _skinRestPosition;
    [Export]
    private ProceduralAnimator _proceduralAnimator;
    [Export]
    private Camera3D _camera;
    [Export]
    private CollisionShape3D _collisionShapeBody;
    private Vector3 _collisionPivot;

    [ExportCategory("Land Movement")]
    [Export]
    public float moveSpeed = 3.0f;

    [Export]
    public float jumpSpeed = 7.0f;

    [Export]
    public float weight = 2.0f;

    [Export]
    public float rotationSpeed = 10.0f;

    [ExportCategory("Swim Movement")]
    [Export]
    public float swimSpeed = 10.0f;

    [Export]
    public float swimRotationSpeed = 5.0f;

    [Export]
    public float swimDamping = 5.0f;

    private bool _swimming = false;
    private float _swimYaw = 0f;
    private float _swimPitch = 0f;

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
    public float flySpeed = 2.0f;

    private bool _gravityEnabled = true;
    private float _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    [Export]
    public bool GravityEnabled
    {
        get => _gravityEnabled;
        set
        {
            _gravityEnabled = value;
            if (!_gravityEnabled)
            {
                Vector3 velo = Velocity;
                velo.Y = 0;
                Velocity = velo;
            }
        }
    }

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

    public override void _Ready()
    {
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
        _camera ??= GetNode<Camera3D>("%Camera3D");
        
        _skin ??= GetNode<Node3D>("Skin");
        _skinRestPosition = _skin.Position;
        
        _proceduralAnimator ??= GetNode<ProceduralAnimator>("ProceduralAnimator");

        _collisionShapeBody ??= GetNode<CollisionShape3D>("CollisionShapeBody");
        _collisionPivot = _collisionShapeBody.Position;
    }

    public override void _Process(double delta)
    {
        if (Input.IsKeyPressed(Key.KpAdd) || Input.IsKeyPressed(Key.Equal))
            moveSpeed = Mathf.Clamp(moveSpeed + 0.5f, 5, 9999);
        if (Input.IsKeyPressed(Key.KpSubtract) || Input.IsKeyPressed(Key.Minus))
            moveSpeed = Mathf.Clamp(moveSpeed - 0.5f, 5, 9999);
    }

    public override void _PhysicsProcess(double delta)
    {
        float pDelta = (float)delta;

        if (_swimming)
        {
            ProcessSwimming(pDelta);
            return;
        }

        if (!_gravityEnabled)
        {
            ProcessFlying(pDelta);
            return;
        }

        ProcessWalking(pDelta);
    }

    private void ProcessWalking(float delta)
    {
        Vector3 direction = GetCameraRelativeDirection();
        Vector2 hVeloc = new Vector2(direction.X, direction.Z).Normalized() * moveSpeed;

        if (direction != Vector3.Zero)
        {
            float targetAngle = Mathf.Atan2(direction.X, direction.Z);
            Vector3 skinRot = _skin.Rotation;
            skinRot.Y = Mathf.LerpAngle(skinRot.Y, targetAngle, rotationSpeed * delta);
            _skin.Rotation = skinRot;
        }

        Vector3 velocity = Velocity;

        if (Input.IsActionPressed("jump") && IsOnFloor())
            velocity.Y = jumpSpeed;

        if (Input.IsKeyPressed(Key.Shift))
            hVeloc *= 2;

        velocity.X = hVeloc.X;
        velocity.Z = hVeloc.Y;

        if (!IsOnFloor())
            velocity.Y -= _gravity * weight * delta;

        Velocity = velocity;
        MoveAndSlide();
    }

    private void ProcessFlying(float delta)
    {
        Vector3 direction = GetCameraRelativeDirection();
        Vector2 hVeloc = new Vector2(direction.X, direction.Z).Normalized() * moveSpeed;

        if (direction != Vector3.Zero)
        {
            float targetAngle = Mathf.Atan2(direction.X, direction.Z);
            Vector3 skinRot = _skin.Rotation;
            skinRot.Y = Mathf.LerpAngle(skinRot.Y, targetAngle, rotationSpeed * delta);
            _skin.Rotation = skinRot;
        }

        Vector3 velocity = Velocity;

        if (Input.IsActionPressed("move_modifier"))
            hVeloc *= 2;

        velocity.X = hVeloc.X;
        velocity.Z = hVeloc.Y;

        if (Input.IsKeyPressed(Key.E))
            velocity.Y += flySpeed + moveSpeed * 0.016f;
        if (Input.IsKeyPressed(Key.Q))
            velocity.Y -= flySpeed + moveSpeed * 0.016f;

        Velocity = velocity;
        MoveAndSlide();
    }

    private void ProcessSwimming(float delta)
    {
        float yawInput = Input.GetActionStrength("right") - Input.GetActionStrength("left");
        float pitchInput = Input.GetActionStrength("up") - Input.GetActionStrength("down");

        _swimYaw -= yawInput * swimRotationSpeed * delta;
        _swimPitch -= pitchInput * swimRotationSpeed * delta;

        UpdateBodyRotation(new Vector3(_swimPitch, _swimYaw, 0f));

        if (Input.IsActionPressed("move_modifier"))
            Velocity = _skin.GlobalTransform.Basis.Y * swimSpeed;
        else
            Velocity = Velocity.Lerp(Vector3.Zero, swimDamping * delta);

        MoveAndSlide();
    }

    private void UpdateBodyRotation(Vector3 rotation)
    {
        Basis rotBasis = Basis.FromEuler(rotation);

        _collisionShapeBody.Rotation = rotation;

        // skin should pivot around the capsule center instead of the feet
        _skin.Rotation = rotation;
        _skin.Position = _collisionPivot + rotBasis * (_skinRestPosition - _collisionPivot);
    }

    private void EnterSwimMode()
    {
        _swimming = true;
        GravityEnabled = false;
        _proceduralAnimator.Enabled = false;
        _swimYaw = _skin.Rotation.Y;
        _swimPitch = _skin.Rotation.X;
    }

    private void ExitSwimMode()
    {
        _swimming = false;
        GravityEnabled = true;
        _proceduralAnimator.Enabled = true;
        Vector3 rotation = _skin.Rotation;
        rotation.X = 0f;
        UpdateBodyRotation(rotation);
        _swimPitch = 0f;
    }

    // Returns the input vector relative to the camera. Forward is always the direction the camera is facing
    private Vector3 GetCameraRelativeDirection()
    {
        Vector3 inputDir = Vector3.Zero;

        inputDir -= _camera.GlobalTransform.Basis.X * Input.GetActionStrength("left");
        inputDir += _camera.GlobalTransform.Basis.X * Input.GetActionStrength("right");
        inputDir -= _camera.GlobalTransform.Basis.Z * Input.GetActionStrength("up");
        inputDir += _camera.GlobalTransform.Basis.Z * Input.GetActionStrength("down");

        return inputDir;
    }

    public override void _Input(InputEvent pEvent)
    {
        if (pEvent is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            switch (mouseEvent.ButtonIndex)
            {
                case MouseButton.WheelUp:
                    moveSpeed = Mathf.Clamp(moveSpeed, 2, 500);
                    break;
                case MouseButton.WheelDown:
                    moveSpeed = Mathf.Clamp(moveSpeed, 2, 500);
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
                    GravityEnabled = !GravityEnabled;
                    break;
                case Key.C:
                    CollisionEnabled = !CollisionEnabled;
                    break;
                case Key.Q:
                case Key.E:
                    Vector3 v = Velocity;
                    v.Y = 0;
                    Velocity = v;
                    break;
                case Key.F:
                    if (_swimming)
                        ExitSwimMode();
                    else
                        EnterSwimMode();
                    break;
            }
        }
    }
}
