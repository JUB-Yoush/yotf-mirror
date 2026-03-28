using Godot;

public partial class PlayerController : CharacterBody3D
{
    [Export] public float MoveSpeed = 10.0f;
    [Export] public float FlySpeed = 2.0f;
    [Export] public float JumpSpeed = 10.0f;
    [Export] public float Weight = 2.0f;
    [Export] public float RotationSpeed = 10.0f;

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
                tween.TweenProperty(GetNode<SpringArm3D>("CameraManager/Arm"), "spring_length", 0.0f, 0.33);
                tween.TweenCallback(Callable.From(() => GetNode<Node3D>("Body").Visible = false));
            }
            else
            {
                GetNode<Node3D>("Body").Visible = true;
                CreateTween().TweenProperty(GetNode<SpringArm3D>("CameraManager/Arm"), "spring_length", 6.0f, 0.33);
            }
        }
    }

    private bool _gravityEnabled = true;
    [Export]
    public bool GravityEnabled
    {
        get => _gravityEnabled;
        set
        {
            _gravityEnabled = value;
            if (!_gravityEnabled)
            {
                Vector3 v = Velocity;
                v.Y = 0;
                Velocity = v;
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

    [Export] private Node3D _skin;
    private float _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    public override void _PhysicsProcess(double delta)
    {
        float pDelta = (float)delta;
        Vector3 direction = GetCameraRelativeInput();
        Vector2 hVeloc = new Vector2(direction.X, direction.Z).Normalized() * MoveSpeed;

        if (direction != Vector3.Zero)
        {
            float targetAngle = Mathf.Atan2(direction.X, direction.Z);
            Vector3 skinRot = _skin.Rotation;
            skinRot.Y = Mathf.LerpAngle(skinRot.Y, targetAngle, RotationSpeed * pDelta);
            _skin.Rotation = skinRot;
        }

        Vector3 velocity = Velocity;

        if (Input.IsActionPressed("jump") && IsOnFloor())
            velocity.Y = JumpSpeed;

        if (Input.IsActionJustPressed("quit"))
            GetTree().Quit();

        if (Input.IsKeyPressed(Key.Shift))
            hVeloc *= 2;

        velocity.X = hVeloc.X;
        velocity.Z = hVeloc.Y;

        if (_gravityEnabled && !IsOnFloor())
            velocity.Y -= _gravity * Weight * pDelta;

        Velocity = velocity;
        MoveAndSlide();
    }

    // Returns the input vector relative to the camera. Forward is always the direction the camera is facing
    private Vector3 GetCameraRelativeInput()
    {
        Vector3 inputDir = Vector3.Zero;
        Camera3D camera = GetNode<Camera3D>("%Camera3D");

        if (Input.IsActionPressed("left"))
            inputDir -= camera.GlobalTransform.Basis.X;
        if (Input.IsActionPressed("right"))
            inputDir += camera.GlobalTransform.Basis.X;
        if (Input.IsActionPressed("up"))
            inputDir -= camera.GlobalTransform.Basis.Z;
        if (Input.IsActionPressed("down"))
            inputDir += camera.GlobalTransform.Basis.Z;

        Vector3 velocity = Velocity;
        if (Input.IsKeyPressed(Key.E))
            velocity.Y += FlySpeed + MoveSpeed * 0.016f;
        if (Input.IsKeyPressed(Key.Q))
            velocity.Y -= FlySpeed + MoveSpeed * 0.016f;
        Velocity = velocity;

        if (Input.IsKeyPressed(Key.KpAdd) || Input.IsKeyPressed(Key.Equal))
            MoveSpeed = Mathf.Clamp(MoveSpeed + 0.5f, 5, 9999);
        if (Input.IsKeyPressed(Key.KpSubtract) || Input.IsKeyPressed(Key.Minus))
            MoveSpeed = Mathf.Clamp(MoveSpeed - 0.5f, 5, 9999);

        return inputDir;
    }

    public override void _Input(InputEvent pEvent)
    {
        if (pEvent is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
                MoveSpeed = Mathf.Clamp(MoveSpeed + 5, 5, 9999);
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
                MoveSpeed = Mathf.Clamp(MoveSpeed - 5, 5, 9999);
        }
        else if (pEvent is InputEventKey keyEvent)
        {
            if (keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.V)
                    FirstPerson = !FirstPerson;
                else if (keyEvent.Keycode == Key.G)
                    GravityEnabled = !GravityEnabled;
                else if (keyEvent.Keycode == Key.C)
                    CollisionEnabled = !CollisionEnabled;
            }
            else if (keyEvent.Keycode == Key.Q || keyEvent.Keycode == Key.E)
            {
                Vector3 v = Velocity;
                v.Y = 0;
                Velocity = v;
            }
        }
    }
}
