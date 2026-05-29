using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class ProceduralAnimator : Node3D
{
    public override void _Notification(int what) => this.Notify(what);

    [Node]
    public required RayCast3D RayCastLeft { set; get; }

    [Node]
    public required RayCast3D RayCastRight { set; get; }

    [Node]
    public required Marker3D FootTargetLeft { set; get; }

    [Node]
    public required Marker3D FootTargetRight { set; get; }

    [Node]
    public required Skeleton3D Skeleton3D { set; get; }

    private Player Player = null!;

    private CameraManager Camera = null!;

    private const int SpineBoneIdx = 1;
    private const int ChestBoneIdx = 22;
    private const int NeckBoneIdx = 35;

    // ======================== HEAD LOOK =======================
    private const float HeadLookMaxAngle = Mathf.Pi / 6f;
    private const float HeadLookWeight = 0.2f;
    private const float HeadLookSpeed = 2f;
    private float currentHeadYaw = 0f;

    // ======================== WALKING =======================
    private const float PredictionScale = 0.6f;
    private const float StepThreshold = 1f;
    private const float StepHeight = 0.2f;
    private const float StepSpeed = 3.0f;
    private const float HipBobAmount = 0.05f;

    // ======================== SWIMMING =======================
    private const float SwimPaddleFrequency = 2.0f;
    private const float SwimPaddleAmplitude = 0.15f;
    private float swimTime = 0f;

    private class FootState
    {
        public Vector3 PlantedPos;
        public Vector3 StepStartPos;
        public Vector3 StepTargetPos;
        public Vector3 DesiredPos;
        public bool IsStepping;
        public float StepT;
    }

    private FootState footStateL = null!;
    private FootState footStateR = null!;
    private Vector3 restingPosL;
    private Vector3 restingPosR;
    private float rootBoneRestY;

    private PlayerState state = PlayerState.Walking;

    public override void _Ready()
    {
        Player = this.SceneRoot().GetNode<Player>()!;
        Camera = Player.GetNode<CameraManager>()!;

        rootBoneRestY = Skeleton3D.GetBonePosePosition(SpineBoneIdx).Y;

        restingPosL = FootTargetLeft.Position;
        restingPosR = FootTargetRight.Position;

        Vector3 worldRestL = ToGlobal(restingPosL);
        Vector3 worldRestR = ToGlobal(restingPosR);

        footStateL = new FootState
        {
            PlantedPos = worldRestL,
            StepStartPos = worldRestL,
            StepTargetPos = worldRestL,
            DesiredPos = worldRestL,
            IsStepping = false,
            StepT = 1f,
        };

        footStateR = new FootState
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
        switch (state)
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
        if (state != PlayerState.Swimming)
            UpdateDesiredPositions();
    }

    internal void OnStateChanged(PlayerState newState)
    {
        state = newState;
        if (newState == PlayerState.Swimming)
        {
            Vector3 currSpinePos = Skeleton3D.GetBonePosePosition(SpineBoneIdx);
            currSpinePos.Y = rootBoneRestY;
            Skeleton3D.SetBonePosePosition(SpineBoneIdx, currSpinePos);

            Quaternion currChestRot = Skeleton3D.GetBonePoseRotation(ChestBoneIdx);
            currChestRot.X = -Mathf.DegToRad(20f);
            Skeleton3D.SetBonePoseRotation(ChestBoneIdx, currChestRot);

            Quaternion currNeckRot = Skeleton3D.GetBonePoseRotation(NeckBoneIdx);
            currNeckRot.X = -Mathf.DegToRad(10f);
            Skeleton3D.SetBonePoseRotation(NeckBoneIdx, currNeckRot);

            swimTime = 0f;
        }
        else
        {
            Vector3 currentSpinePos = Skeleton3D.GetBonePosePosition(SpineBoneIdx);
            currentSpinePos.Y = rootBoneRestY;
            Skeleton3D.SetBonePosePosition(SpineBoneIdx, currentSpinePos);

            Quaternion currentChestRot = Skeleton3D.GetBonePoseRotation(ChestBoneIdx);
            currentChestRot.X = 0f;
            Skeleton3D.SetBonePoseRotation(ChestBoneIdx, currentChestRot);

            Quaternion currentNeckRot = Skeleton3D.GetBonePoseRotation(NeckBoneIdx);
            currentNeckRot.X = 0f;
            Skeleton3D.SetBonePoseRotation(NeckBoneIdx, currentNeckRot);

            // Prevent IK snap when re-entering walk/fly
            FootTargetLeft.GlobalPosition = footStateL.PlantedPos;
            FootTargetRight.GlobalPosition = footStateR.PlantedPos;
        }
    }

    private void ProcessWalkAnimation(float delta)
    {
        InterpolateStep(footStateL, FootTargetLeft, delta);
        InterpolateStep(footStateR, FootTargetRight, delta);
        InterpolateHips();

        if (ShouldStep(footStateL, footStateR))
            TriggerStep(footStateL);

        if (ShouldStep(footStateR, footStateL))
            TriggerStep(footStateR);
    }

    private void ProcessSwimAnimation(float delta)
    {
        UpdateHeadLook(delta);

        Vector3 velocity = Player.Velocity;

        swimTime += delta;
        float angle = swimTime * SwimPaddleFrequency * Mathf.Tau;
        float velocityScale = velocity.Length() / Player.SwimSpeed;

        // alternating sine wave for feet paddling
        float leftY = Mathf.Cos(angle) * StepHeight * velocityScale;
        float leftZ = (Mathf.Sin(angle) - 1f) * SwimPaddleAmplitude * velocityScale;

        float rightY = Mathf.Cos(angle + Mathf.Pi) * StepHeight * velocityScale;
        float rightZ = (Mathf.Sin(angle + Mathf.Pi) - 1f) * SwimPaddleAmplitude * velocityScale;

        FootTargetLeft.Position = restingPosL + new Vector3(0f, leftY, leftZ);
        FootTargetRight.Position = restingPosR + new Vector3(0f, rightY, rightZ);
    }

    private void UpdateDesiredPositions()
    {
        Vector3 velocity = Player.Velocity;

        Vector3 worldRestL = ToGlobal(restingPosL);
        Vector3 worldRestR = ToGlobal(restingPosR);

        footStateL.DesiredPos = RayCastLeft.IsColliding()
            ? RayCastLeft.GetCollisionPoint()
            : worldRestL;

        footStateR.DesiredPos = RayCastRight.IsColliding()
            ? RayCastRight.GetCollisionPoint()
            : worldRestR;

        if (velocity != Vector3.Zero)
        {
            Vector3 prediction = velocity.Normalized() * PredictionScale;
            footStateL.DesiredPos += prediction;
            footStateR.DesiredPos += prediction;
        }
    }

    private static bool ShouldStep(FootState foot, FootState otherFoot)
    {
        if (otherFoot.IsStepping)
            return false;

        bool meetsThreshold = foot.PlantedPos.DistanceTo(foot.DesiredPos) > StepThreshold;
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
        float swingL = Mathf.Sin(footStateL.StepT * Mathf.Pi);
        float swingR = Mathf.Sin(footStateR.StepT * Mathf.Pi);
        float bobOffset = Mathf.Max(swingL, swingR) * HipBobAmount;
        Skeleton3D.SetBonePosePosition(
            SpineBoneIdx,
            Skeleton3D.GetBonePosePosition(SpineBoneIdx) with
            {
                Y = rootBoneRestY - bobOffset,
            }
        );
    }

    private void InterpolateStep(FootState foot, Marker3D footTarget, float delta)
    {
        if (foot.StepT < 1f)
        {
            Vector3 newPosition = foot.StepStartPos.Lerp(foot.StepTargetPos, foot.StepT);
            newPosition.Y += Mathf.Sin(foot.StepT * Mathf.Pi) * StepHeight;

            foot.StepT += delta * Player.MoveSpeed * StepSpeed;

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
        if (!Mathf.IsZeroApprox(Player.YawVelocity))
            return;

        float cameraYaw = Camera.Rotation.Y;
        float bodyYaw = Player.Skin.Rotation.Y;

        float targetYaw = Mathf.AngleDifference(bodyYaw, cameraYaw) * HeadLookWeight;
        targetYaw = Mathf.Clamp(targetYaw, -HeadLookMaxAngle, HeadLookMaxAngle);

        currentHeadYaw = Mathf.LerpAngle(currentHeadYaw, -targetYaw, HeadLookSpeed * delta);

        Quaternion headRot = Skeleton3D.GetBonePoseRotation(NeckBoneIdx);
        headRot.Y = currentHeadYaw;

        Skeleton3D.SetBonePoseRotation(NeckBoneIdx, headRot);
    }
}
