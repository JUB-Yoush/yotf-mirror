using Godot;

public partial class ProceduralAnimator : Node3D
{
    private PlayerController _player;

    [Export]
    private BoneAttachment3D _spineBone;

    private const float _predictionScale = 0.8f;
    private const float _stepThreshold = 1f;
    private const float _stepHeight = 0.2f;
    private const float _stepSpeed = 3.0f;
    private const float _hipBobAmount = 0.05f;

    [Export]
    private RayCast3D _rayCastL;

    [Export]
    private RayCast3D _rayCastR;

    [Export]
    private Marker3D _footTargetL;

    [Export]
    private Marker3D _footTargetR;

    private class FootState
    {
        public Vector3 plantedPos;
        public Vector3 stepStartPos;
        public Vector3 stepTargetPos;
        public Vector3 desiredPos;
        public bool isStepping;
        public float stepT;
    }

    private FootState _footStateL;
    private FootState _footStateR;
    private Vector3 _restingPosL;
    private Vector3 _restingPosR;
    private float _rootBoneRestY;

    private bool _enabled = true;
    public bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            if (!_enabled)
                _spineBone.Position = _spineBone.Position with { Y = _rootBoneRestY };
        }
    }

    public override void _Ready()
    {
        _player ??= GetParent<PlayerController>();
        _rayCastL ??= GetNode<RayCast3D>("../Skin/Rig/FootRaycast_Left");
        _rayCastR ??= GetNode<RayCast3D>("../Skin/Rig/FootRaycast_Right");
        _footTargetL ??= GetNode<Marker3D>("../Skin/Rig/FootTarget_Left");
        _footTargetR ??= GetNode<Marker3D>("../Skin/Rig/FootTarget_Right");
        _spineBone ??= GetNode<BoneAttachment3D>("../Skin/Rig/Skeleton3D/Spine");

        _rootBoneRestY = _spineBone.Position.Y;

        _restingPosL = _footTargetL.Position;
        _restingPosR = _footTargetR.Position;

        Vector3 worldRestL = ToGlobal(_restingPosL);
        Vector3 worldRestR = ToGlobal(_restingPosR);

        _footStateL = new FootState
        {
            plantedPos = worldRestL,
            stepStartPos = worldRestL,
            stepTargetPos = worldRestL,
            desiredPos = worldRestL,
            isStepping = false,
            stepT = 1f,
        };

        _footStateR = new FootState
        {
            plantedPos = worldRestR,
            stepStartPos = worldRestR,
            stepTargetPos = worldRestR,
            desiredPos = worldRestR,
            isStepping = false,
            stepT = 1f,
        };
    }

    public override void _Process(double delta)
    {
        if (!_enabled)
            return;
        InterpolateStep(_footStateL, _footTargetL, delta);
        InterpolateStep(_footStateR, _footTargetR, delta);
        InterpolateHips();

        if (ShouldStep(_footStateL, _footStateR))
        {
            GD.Print(
                "Stepping left, distance: ",
                _footStateL.plantedPos.DistanceTo(_footStateL.desiredPos)
            );
            TriggerStep(_footStateL);
        }

        if (ShouldStep(_footStateR, _footStateL))
        {
            GD.Print(
                "Stepping right, distance: ",
                _footStateR.plantedPos.DistanceTo(_footStateR.desiredPos)
            );
            TriggerStep(_footStateR);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_enabled)
            return;
        UpdateDesiredPositions();
    }

    private void UpdateDesiredPositions()
    {
        Vector3 worldRestL = GlobalTransform * _restingPosL;
        Vector3 worldRestR = GlobalTransform * _restingPosR;

        _footStateL.desiredPos = _rayCastL.IsColliding()
            ? _rayCastL.GetCollisionPoint()
            : worldRestL;

        _footStateR.desiredPos = _rayCastR.IsColliding()
            ? _rayCastR.GetCollisionPoint()
            : worldRestR;

        _footStateL.desiredPos += _player.Velocity.Normalized() * _predictionScale;
        _footStateR.desiredPos += _player.Velocity.Normalized() * _predictionScale;
    }

    private static bool ShouldStep(FootState foot, FootState otherFoot)
    {
        if (otherFoot.isStepping)
            return false;

        bool meetsThreshold = foot.plantedPos.DistanceTo(foot.desiredPos) > _stepThreshold;
        return meetsThreshold;
    }

    private static void TriggerStep(FootState foot)
    {
        foot.stepStartPos = foot.plantedPos;
        foot.stepTargetPos = foot.desiredPos;
        foot.plantedPos = foot.desiredPos;
        foot.isStepping = true;
        foot.stepT = 0f;
    }

    private void InterpolateHips()
    {
        float swingL = Mathf.Sin(_footStateL.stepT * Mathf.Pi);
        float swingR = Mathf.Sin(_footStateR.stepT * Mathf.Pi);
        float bobOffset = Mathf.Max(swingL, swingR) * _hipBobAmount;
        _spineBone.Position = _spineBone.Position with { Y = _rootBoneRestY - bobOffset };
    }

    private void InterpolateStep(FootState foot, Marker3D footTarget, double delta)
    {
        if (foot.stepT < 1f)
        {
            Vector3 newPosition = foot.stepStartPos.Lerp(foot.stepTargetPos, foot.stepT);
            newPosition.Y += Mathf.Sin(foot.stepT * Mathf.Pi) * _stepHeight;

            foot.stepT += (float)delta * _player.moveSpeed * _stepSpeed;

            footTarget.GlobalPosition = newPosition;
        }
        else
        {
            footTarget.GlobalPosition = foot.plantedPos;
            foot.isStepping = false;
        }
    }
}
