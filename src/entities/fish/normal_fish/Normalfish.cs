using System;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Normalfish : Fish, IHearNoise, IBubbleable
{
    public override void _Notification(int what) => this.Notify(what);

    public enum State
    {
        Wander,
        Flee,
        Bubbled,
    }

    //fleeing
    private float fleeTimer;

    StateMachine<State> stateMachine = new();

    public Bubble? BubbleJail { get; set; }

    Mesh IBubbleable.Mesh => Mesh.Mesh;

    public override void _Ready()
    {
        navGraph = this.SceneRoot().GetNode<NavGraph>()!;
        stateMachine.AddState(State.Wander, WanderUpdate, WanderEnter);
        stateMachine.AddState(State.Flee, FleeUpdate);
        stateMachine.AddState(State.Bubbled, BubbleUpdate);
        stateMachine.State = State.Wander;
    }

    public override void _PhysicsProcess(double delta)
    {
        stateMachine.Update(delta);
        MoveAndSlide();
    }

    public void BubbleUpdate(float delta)
    {
        GlobalPosition = BubbleJail!.GlobalPosition;
        Velocity = Vector3.Zero;
    }

    private void FleeUpdate(float delta)
    {
        if (ThreatTarget != null)
            ThreatPosition = ThreatTarget.GlobalPosition;

        Vector3 awayDir = (GlobalPosition - ThreatPosition).Normalized();
        Vector3 fleeTarget = GlobalPosition + awayDir * FleeDistance;

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
}
