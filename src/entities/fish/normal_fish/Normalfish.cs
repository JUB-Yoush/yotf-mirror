using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Normalfish : Fish, IHearNoise, IBubbleable, ISonarable, ITakeDamage
{
    public override void _Notification(int what) => this.Notify(what);

    public enum State
    {
        Wander,
        Flee,
        Bubbled,
        Baited,
    }

    //baited
    private Area3D? bait;

    //fleeing
    private float fleeTimer;

    readonly StateMachine<State> stateMachine = new();

    public Bubble? BubbleJail { get; set; }

    Mesh IBubbleable.Mesh => Mesh.Mesh;

    public override void _Ready()
    {
        base._Ready();
        stateMachine.AddState(State.Wander, WanderUpdate, WanderEnter);
        stateMachine.AddState(State.Flee, FleeUpdate);
        stateMachine.AddState(State.Bubbled, BubbleUpdate);
        stateMachine.AddState(State.Baited, BaitedUpdate, exit: BaitedExit);
        stateMachine.State = State.Wander;

        DetectionZone.BodyEntered += OnDetectionBodyEntered;
        DetectionZone.BodyExited += OnDetectionBodyExited;
        DetectionZone.AreaEntered += OnDetectionAreaEntered;
        DetectionZone.AreaExited += OnDetectionAreaExited;
    }

    private void OnDetectionBodyEntered(Node3D body)
    {
        if (body is Player player && stateMachine.State != State.Bubbled)
        {
            stateMachine.State = State.Flee;
            ThreatTarget = player;
        }
    }

    private void OnDetectionBodyExited(Node3D body) { }

    private void OnDetectionAreaEntered(Area3D area)
    {
        if (
            area is Bait baitArea
            && stateMachine.State != State.Flee
            && stateMachine.State != State.Bubbled
        )
        {
            stateMachine.State = State.Baited;
            bait = baitArea;
        }
    }

    private void OnDetectionAreaExited(Area3D area) { }

    private void BaitedExit()
    {
        bait = null;
    }

    private void BaitedUpdate(float delta)
    {
        if (!bait!.IsValid())
        {
            stateMachine.State = State.Wander;
            return;
        }

        if (SmoothMoveTo(bait!.GlobalPosition, WanderSpeed, delta, .1f))
        {
            bait.QueueFree();
            stateMachine.State = State.Wander;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsKeyPressed(Key.Space))
        {
            Log.PrintLn("dead");
            IsDead = true;
        }
        if (AIIsOn && !IsDead)
            stateMachine.Update(delta);
        MoveAndSlide();
    }

    public void BubbleUpdate(float delta)
    {
        if (GodotObject.IsInstanceValid(BubbleJail))
        {
            GlobalPosition = BubbleJail!.GlobalPosition;
        }
        Velocity = Vec3.Zero;
    }

    private void FleeUpdate(float delta)
    {
        if (GodotObject.IsInstanceValid(ThreatTarget))
            ThreatPosition = ThreatTarget.GlobalPosition;

        Vec3 awayDir = (GlobalPosition - ThreatPosition).Normalized();
        Vec3 fleeTarget = GlobalPosition + awayDir * FleeDistance;

        bool arrived = SmoothMoveTo(fleeTarget, FleeSpeed, delta, ArrivalThreshold);
        fleeTimer += delta;

        if (arrived || fleeTimer >= FleeTimeout)
        {
            stateMachine.State = State.Wander;
        }
    }

    public void WanderEnter()
    {
        CurrentNode = navGraph.NodeClosestTo(GlobalPosition);
        CurrentRoom = AssignCurrentRoom();
    }

    private void WanderUpdate(float delta)
    {
        if (SmoothMoveTo(CurrentNode!.GlobalPosition, WanderSpeed, delta, ArrivalThreshold))
        {
            CurrentNode = PickWanderTarget();
        }
    }

    public void OnNoiseHeard(Node3D NoiseSource, float dB, string noise)
    {
        stateMachine.State = State.Flee;
        ThreatTarget = NoiseSource;
    }

    public void PutInBubble()
    {
        stateMachine.State = State.Bubbled;
        Mesh.Visible = false;
        DetectionZone.Monitoring = false;
    }

    public void FreeFromBubble()
    {
        stateMachine.State = State.Wander;
        Mesh.Visible = true;
        DetectionZone.Monitoring = true;
    }

    public override void FoundBait(Bait bait)
    {
        OnDetectionAreaEntered(bait);
    }

    void ITakeDamage.OnDamageTaken(float amount, Vec3 knockback, Node3D source)
    {
        stateMachine.State = State.Flee;
        ThreatTarget = source;
    }
}
