using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Anglerfish : Fish, IBubbleable, IHearNoise, IDoesAction, ITakeDamage
{
    public override void _Notification(int what) => this.Notify(what);

    public Bubble? BubbleJail { get; set; }
    public bool InAction { get; set; }

    Vector3 NoisePosition;

    Mesh IBubbleable.Mesh => Mesh.Mesh;

    [Node]
    public required Area3D HitBox { set; get; }

    [Export]
    float chaseSpeed = 20f;

    [Export]
    float chaseKnockback = 25f;

    [Export]
    float chaseDmg = 20f;

    [Export]
    float chaseTime = 10f;

    float currentchaseTime = 10f;

    readonly HashSet<ITakeDamage> hit = [];

    enum State
    {
        Idle,
        Chasing,
        Bubbled,
    }

    private readonly StateMachine<State> stateMachine = new();

    public override void _Ready()
    {
        base._Ready();

        navGraph = this.SceneRoot().GetNode<NavGraph>()!;
        stateMachine.AddState(State.Idle, IdleUpdate, IdleEnter);
        stateMachine.AddState(State.Bubbled, BubbleUpdate);
        stateMachine.AddState(State.Chasing, ChasingUpdate, exit: ChasingExit);
        HitBox.BodyEntered += OnHitboxBodyEntered;
        stateMachine.State = State.Idle;
    }

    private void BubbleUpdate(float delta)
    {
        if (GodotObject.IsInstanceValid(BubbleJail))
        {
            GlobalPosition = BubbleJail!.GlobalPosition;
        }
        Velocity = Vec3.Zero;
    }

    private void OnHitboxBodyEntered(Node3D body)
    {
        if (stateMachine.State != State.Chasing)
            return;

        if (body is ITakeDamage damageTaker && hit.Add(damageTaker))
        {
            hit.Add(damageTaker);
            var kb =
                (damageTaker.Node.GlobalPosition - GlobalPosition).Normalized() * chaseKnockback;
            damageTaker.TakeDamage(chaseDmg, kb, this);
        }
    }

    private void ChasingUpdate(float delta)
    {
        if (SmoothMoveTo(NoisePosition, chaseSpeed, delta, 5))
        {
            stateMachine.State = State.Idle;
        }
        currentchaseTime -= delta;
        if (currentchaseTime == 0)
        {
            stateMachine.State = State.Idle;
        }
    }

    private void ChasingExit()
    {
        currentchaseTime = chaseTime;
        hit.Clear();
    }

    public override void _Process(double delta)
    {
        stateMachine.Update(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();
    }

    public void IdleEnter()
    {
        CurrentNode = navGraph.NodeClosestTo(GlobalPosition);
        CurrentRoom = AssignCurrentRoom();
    }

    private void IdleUpdate(float delta)
    {
        if (SmoothMoveTo(CurrentNode!.GlobalPosition, WanderSpeed, delta, ArrivalThreshold))
        {
            CurrentNode = PickWanderTarget();
        }
    }

    public void PutInBubble()
    {
        stateMachine.State = State.Bubbled;
        Mesh.Visible = false;
        DetectionZone.Monitoring = false;
    }

    public void FreeFromBubble()
    {
        stateMachine.State = State.Idle;
        Mesh.Visible = true;
        DetectionZone.Monitoring = true;
    }

    public void OnNoiseHeard(Node3D NoiseSource, float dB, string noise)
    {
        if (stateMachine.State != State.Bubbled)
        {
            stateMachine.State = State.Chasing;
            NoisePosition = NoiseSource.GlobalPosition;
        }
    }
}
