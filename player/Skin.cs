using Godot;

public partial class Skin : Node3D
{
    [Export]
    public bool Blink
    {
        get => _blink;
        set => SetBlink(value);
    }

    private bool _blink = true;
    private AnimationTree _animationTree;
    private AnimationNodeStateMachinePlayback _stateMachine;
    private string _moveTiltPath = "parameters/StateMachine/Move/tilt/add_amount";

    private float _runTilt = 0.0f;
    public float RunTilt
    {
        get => _runTilt;
        set
        {
            _runTilt = Mathf.Clamp(value, -1.0f, 1.0f);
            _animationTree.Set(_moveTiltPath, _runTilt);
        }
    }

    private Timer _blinkTimer;
    private Timer _closedEyesTimer;
    private ShaderMaterial _eyeMat;

    public override void _Ready()
    {
        _animationTree = GetNode<AnimationTree>("%AnimationTree");
        _stateMachine = (AnimationNodeStateMachinePlayback)_animationTree.Get("parameters/StateMachine/playback");

        _blinkTimer = GetNode<Timer>("%BlinkTimer");
        _closedEyesTimer = GetNode<Timer>("%ClosedEyesTimer");
        _eyeMat = (ShaderMaterial)GetNode<MeshInstance3D>("sophia/rig/Skeleton3D/Sophia").GetSurfaceOverrideMaterial(2);

        _blinkTimer.Timeout += OnBlinkTimeout;
        _closedEyesTimer.Timeout += OnClosedEyesTimeout;
    }

    private void OnBlinkTimeout()
    {
        _eyeMat.Set("uv1_offset", new Vector3(0.0f, 0.5f, 0.0f));
        _closedEyesTimer.Start(0.2);
    }

    private void OnClosedEyesTimeout()
    {
        _eyeMat.Set("uv1_offset", Vector3.Zero);
        _blinkTimer.Start(GD.RandRange(1.0, 4.0));
    }

    private void SetBlink(bool state)
    {
        if (_blink == state) return;
        _blink = state;
        if (_blink)
            _blinkTimer.Start(0.2);
        else
        {
            _blinkTimer.Stop();
            _closedEyesTimer.Stop();
        }
    }

    public void Idle() => _stateMachine.Travel("Idle");
    public void Move() => _stateMachine.Travel("Move");
    public void Fall() => _stateMachine.Travel("Fall");
    public void Jump() => _stateMachine.Travel("Jump");
    public void EdgeGrab() => _stateMachine.Travel("EdgeGrab");
    public void WallSlide() => _stateMachine.Travel("WallSlide");
}
