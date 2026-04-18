using Godot;

public partial class ProceduralAnimator : Node3D
{
    private PlayerController _player;

    [Export]
    private Skeleton3D _skeleton;
    [Export]
    private CameraManager _camera;
    private const int _spineBoneIdx = 0;
    private const int _chestBoneIdx = 3;
    private const int _neckBoneIdx = 4;

    // ======================== HEAD LOOK =======================
    private const float _headLookMaxAngle = Mathf.Pi / 6f;
    private const float _headLookWeight = 0.2f;
    private const float _headLookSpeed = 2f;
    private float _currentHeadYaw = 0f;

    // ======================== WALKING =======================
    private const float _predictionScale = 0.6f;
    private const float _stepThreshold = 1f;
    private const float _stepHeight = 0.2f;
    private const float _stepSpeed = 3.0f;
    private const float _hipBobAmount = 0.05f;

    // ======================== SWIMMING =======================
    private const float _swimPaddleFrequency = 2.0f;
    private const float _swimPaddleAmplitude = 0.15f;
    private float _swimTime = 0f;

    [Export]
    private RayCast3D _raycastLeft;

    [Export]
    private RayCast3D _raycastRight;

    [Export]
    private Marker3D _footTargetLeft;

    [Export]
    private Marker3D _footTargetRight;

    private class FootState
    {
        public Vector3 PlantedPos;
        public Vector3 StepStartPos;
        public Vector3 StepTargetPos;
        public Vector3 DesiredPos;
        public bool IsStepping;
        public float StepT;
    }

    private FootState _footStateL;
    private FootState _footStateR;
    private Vector3 _restingPosL;
    private Vector3 _restingPosR;
    private float _rootBoneRestY;

    private PlayerState _state = PlayerState.Walking;

    public override void _Ready()
    {
        _player = GetParent<PlayerController>();

        _rootBoneRestY = _skeleton.GetBonePosePosition(_spineBoneIdx).Y;

        _restingPosL = _footTargetLeft.Position;
        _restingPosR = _footTargetRight.Position;

        Vector3 worldRestL = ToGlobal(_restingPosL);
        Vector3 worldRestR = ToGlobal(_restingPosR);

        _footStateL = new FootState
        {
            PlantedPos = worldRestL,
            StepStartPos = worldRestL,
            StepTargetPos = worldRestL,
            DesiredPos = worldRestL,
            IsStepping = false,
            StepT = 1f,
        };

        _footStateR = new FootState
        {
            PlantedPos = worldRestR,
            StepStartPos = worldRestR,
            StepTargetPos = worldRestR,
            DesiredPos = worldRestR,
            IsStepping = false,
            StepT = 1f,
        };
    }

    public override void _Process(double delta)
    {
        float d = (float)delta;
        switch (_state)
        {
            case PlayerState.Walking:
                ProcessWalkAnimation(d);
                break;
            case PlayerState.Swimming:
                ProcessSwimAnimation(d);
                break;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_state != PlayerState.Swimming)
            UpdateDesiredPositions();
    }

    internal void OnStateChanged(PlayerState newState)
    {
        _state = newState;
        if (newState == PlayerState.Swimming)
        {
            Vector3 currSpinePos = _skeleton.GetBonePosePosition(_spineBoneIdx);
            currSpinePos.Y = _rootBoneRestY;
            _skeleton.SetBonePosePosition(_spineBoneIdx, currSpinePos);

            Quaternion currChestRot = _skeleton.GetBonePoseRotation(_chestBoneIdx);
            currChestRot.X = -Mathf.DegToRad(20f);
            _skeleton.SetBonePoseRotation(_chestBoneIdx, currChestRot);

            Quaternion currNeckRot = _skeleton.GetBonePoseRotation(_neckBoneIdx);
            currNeckRot.X = -Mathf.DegToRad(10f);
            _skeleton.SetBonePoseRotation(_neckBoneIdx, currNeckRot);

            _swimTime = 0f;
        }
        else
        {
            Vector3 currentSpinePos = _skeleton.GetBonePosePosition(_spineBoneIdx);
            currentSpinePos.Y = _rootBoneRestY;
            _skeleton.SetBonePosePosition(_spineBoneIdx, currentSpinePos);

            Quaternion currentChestRot = _skeleton.GetBonePoseRotation(_chestBoneIdx);
            currentChestRot.X = 0f;
            _skeleton.SetBonePoseRotation(_chestBoneIdx, currentChestRot);

            Quaternion currentNeckRot = _skeleton.GetBonePoseRotation(_neckBoneIdx);
            currentNeckRot.X = 0f;
            _skeleton.SetBonePoseRotation(_neckBoneIdx, currentNeckRot);

            // Prevent IK snap when re-entering walk/fly
            _footTargetLeft.GlobalPosition = _footStateL.PlantedPos;
            _footTargetRight.GlobalPosition = _footStateR.PlantedPos;
        }
    }

    private void ProcessWalkAnimation(float delta)
    {
        InterpolateStep(_footStateL, _footTargetLeft, delta);
        InterpolateStep(_footStateR, _footTargetRight, delta);
        InterpolateHips();

        if (ShouldStep(_footStateL, _footStateR))
            TriggerStep(_footStateL);

        if (ShouldStep(_footStateR, _footStateL))
            TriggerStep(_footStateR);
    }

    private void ProcessSwimAnimation(float delta)
    {
        UpdateHeadLook(delta);

        Vector3 velocity = _player.Velocity;

        _swimTime += delta;
        float angle = _swimTime * _swimPaddleFrequency * Mathf.Tau;
        float velocityScale = velocity.Length() / _player.SwimSpeed;

        // alternating sine wave for feet paddling
        float leftY = Mathf.Cos(angle) * _stepHeight * velocityScale;
        float leftZ = (Mathf.Sin(angle) - 1f) * _swimPaddleAmplitude * velocityScale;

        float rightY = Mathf.Cos(angle + Mathf.Pi) * _stepHeight * velocityScale;
        float rightZ = (Mathf.Sin(angle + Mathf.Pi) - 1f) * _swimPaddleAmplitude * velocityScale;

        _footTargetLeft.Position = _restingPosL + new Vector3(0f, leftY, leftZ);
        _footTargetRight.Position = _restingPosR + new Vector3(0f, rightY, rightZ);
    }

    private void UpdateDesiredPositions()
    {
        Vector3 velocity = _player.Velocity;

        Vector3 worldRestL = ToGlobal(_restingPosL);
        Vector3 worldRestR = ToGlobal(_restingPosR);

        _footStateL.DesiredPos = _raycastLeft.IsColliding()
            ? _raycastLeft.GetCollisionPoint()
            : worldRestL;

        _footStateR.DesiredPos = _raycastRight.IsColliding()
            ? _raycastRight.GetCollisionPoint()
            : worldRestR;

        if (velocity != Vector3.Zero)
        {
            Vector3 prediction = velocity.Normalized() * _predictionScale;
            _footStateL.DesiredPos += prediction;
            _footStateR.DesiredPos += prediction;
        }
    }

    private static bool ShouldStep(FootState foot, FootState otherFoot)
    {
        if (otherFoot.IsStepping)
            return false;

        bool meetsThreshold = foot.PlantedPos.DistanceTo(foot.DesiredPos) > _stepThreshold;
        return meetsThreshold;
    }

    private static void TriggerStep(FootState foot)
    {
        foot.StepStartPos = foot.PlantedPos;
        foot.StepTargetPos = foot.DesiredPos;
        foot.PlantedPos = foot.DesiredPos;
        foot.IsStepping = true;
        foot.StepT = 0f;
    }

    private void InterpolateHips()
    {
        float swingL = Mathf.Sin(_footStateL.StepT * Mathf.Pi);
        float swingR = Mathf.Sin(_footStateR.StepT * Mathf.Pi);
        float bobOffset = Mathf.Max(swingL, swingR) * _hipBobAmount;
        _skeleton.SetBonePosePosition(_spineBoneIdx, _skeleton.GetBonePosePosition(_spineBoneIdx) with { Y = _rootBoneRestY - bobOffset });
    }

    private void InterpolateStep(FootState foot, Marker3D footTarget, float delta)
    {
        if (foot.StepT < 1f)
        {
            Vector3 newPosition = foot.StepStartPos.Lerp(foot.StepTargetPos, foot.StepT);
            newPosition.Y += Mathf.Sin(foot.StepT * Mathf.Pi) * _stepHeight;

            foot.StepT += delta * _player.MoveSpeed * _stepSpeed;

            footTarget.GlobalPosition = newPosition;
        }
        else
        {
            footTarget.GlobalPosition = foot.PlantedPos;
            foot.IsStepping = false;
        }
    }

    private void UpdateHeadLook(float delta)
    {
        // don't fight with player rotation when turning
        if (!Mathf.IsZeroApprox(_player.YawVelocity))
            return;

        float cameraYaw = _camera.Rotation.Y;
        float bodyYaw = _player.Skin.Rotation.Y;

        float targetYaw = Mathf.AngleDifference(bodyYaw, cameraYaw) * _headLookWeight;
        targetYaw = Mathf.Clamp(targetYaw, -_headLookMaxAngle, _headLookMaxAngle);

        _currentHeadYaw = Mathf.LerpAngle(_currentHeadYaw, -targetYaw, _headLookSpeed * delta);

        Quaternion headRot = _skeleton.GetBonePoseRotation(_neckBoneIdx);
        headRot.Y = _currentHeadYaw;

        _skeleton.SetBonePoseRotation(_neckBoneIdx, headRot);
    }
}
