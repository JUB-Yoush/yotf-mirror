using Godot;

public partial class Skin : Node3D
{
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

    public override void _Ready()
    {
        _animationTree = GetNode<AnimationTree>("%AnimationTree");
        _stateMachine = (AnimationNodeStateMachinePlayback)
            (GodotObject)_animationTree.Get("parameters/StateMachine/playback");
    }

    public void Idle() => _stateMachine.Travel("Idle");

    public void Move() => _stateMachine.Travel("Move");

    public void Fall() => _stateMachine.Travel("Fall");

    public void Jump() => _stateMachine.Travel("Jump");

    public void EdgeGrab() => _stateMachine.Travel("EdgeGrab");

    public void WallSlide() => _stateMachine.Travel("WallSlide");
}
