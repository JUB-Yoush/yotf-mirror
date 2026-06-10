using System;
using System.Collections.Generic;
using Godot;

namespace Yotf;

[Meta(typeof(IAutoNode))]
public partial class Eel : Fish, IBubbleable, IHearNoise, IDoesAction, ITakeDamage
{
    public override void _Notification(int what) => this.Notify(what);

    [Node]
    public required CollisionShape3D ZapShape { set; get; }

    [Node]
    public required MeshInstance3D ZapMesh { set; get; }

    [Node]
    public required Area3D ZapArea { set; get; }

    [Node]
    public required AnimationPlayer AnimPlayer { set; get; }

    [Node]
    public required AudioStreamPlayer3D SfxSource { set; get; }

    public float Period
    {
        private set { field = (float)Mathf.Wrap(value, 0, 2 * Math.PI); }
        get;
    }

    [Export]
    float zapDamage = 3f;

    [Export]
    float zapDuration = 4f;

    [Export]
    float zapKnockback = 30f;

    public Bubble? BubbleJail { get; set; }

    Mesh IBubbleable.Mesh => Mesh.Mesh;
    float IBubbleable.MeshScale => .3f;

    bool IBubbleable.AxolotlTargets => false;

    public bool InAction { get; set; }
    public int OnBodyEntered { get; private set; }

    HashSet<Node3D> zapped = [];

    Node3D? zapTarget = null;
    Tween? zapTween = null;

    enum State
    {
        Wander,
        Electric,
        Bubbled,
    }

    float wanderTimer = 3f;

    private readonly StateMachine<State> stateMachine = new();

    public override void _Ready()
    {
        base._Ready();
        stateMachine.AddState(State.Wander, WanderUpdate, WanderEnter);
        stateMachine.AddState(State.Electric, ElectricUpdate, ElectricEnter, ElectricExit);
        stateMachine.AddState(State.Bubbled, BubbledUpdate);
        stateMachine.State = State.Wander;
        DetectionZone.BodyEntered += OnDetectionBodyEntered;
    }

    private void OnDetectionBodyEntered(Node3D body)
    {
        Log.PrintLn("eel area entered");
        if (body is Player player && stateMachine.State == State.Wander)
        {
            stateMachine.State = State.Electric;
            zapTarget = player;
        }
    }

    private void BubbledUpdate(float delta)
    {
        if (GodotObject.IsInstanceValid(BubbleJail))
        {
            GlobalPosition = BubbleJail!.GlobalPosition;
        }
        Velocity = Vec3.Zero;
    }

    private void WanderEnter()
    {
        CurrentRoom = AssignCurrentRoom();
        ReturningHome = true;
    }

    private void WanderUpdate(float delta)
    {
        if (ReturningHome)
        {
            ReturningHome = !(
                SmoothMoveTo(CurrentRoom!.GlobalPosition, WanderSpeed, delta, WanderRadius * 5)
            );
            return;
        }

        Period += delta * WanderSpeed;
        var target = new Vec3(
            WanderRadius * MathF.Sin(Period),
            WanderRadius * MathF.Sin(Period * NavRandomOffsetRange),
            WanderRadius * MathF.Cos(Period)
        );
        target += CurrentRoom!.GlobalPosition;
        SmoothMoveTo(target, WanderSpeed, delta);
        // Velocity = new(
        //     (float)-(WanderRadius * Math.Sin(Period)),
        //     0,
        //     (float)-(WanderRadius * -Math.Cos(Period))
        // );
        //elecTimer -= delta;
        if (zapDuration < 0)
        {
            stateMachine.State = State.Electric;
            zapDuration = 3f;
        }
    }

    private void ElectricEnter()
    {
        ZapShape.SetDeferred(CollisionShape3D.PropertyName.Disabled, false);
        InAction = true;
        AnimPlayer.Play("zap");
        Audio.PlaySfx(Sfx.Electric, SfxSource);
    }

    private void ElectricExit()
    {
        zapTarget = null;
        ZapShape.SetDeferred(CollisionShape3D.PropertyName.Disabled, true);
        InAction = false;
        AnimPlayer.Stop();
        ZapMesh.Visible = false;
        zapped.Clear();
        SfxSource.Stop();
    }

    private void ElectricUpdate(float delta)
    {
        if (zapTarget != null && zapTarget.IsValid())
        {
            SmoothMoveTo(zapTarget.GlobalPosition, WanderSpeed * 2, delta);
        }
        foreach (var overlapper in ZapArea.GetOverlappingBodies())
        {
            if (overlapper == this || zapped.Contains(overlapper))
                continue;
            if (overlapper is ITakeDamage damageTaker)
            {
                zapped.Add(overlapper);
                Log.PrintLn(overlapper.Name);
                var kb =
                    (damageTaker.Node.GlobalPosition - GlobalPosition).Normalized() * zapKnockback;
                damageTaker.TakeDamage(3, kb, this);
            }
        }
        zapDuration -= delta;
        if (zapDuration < 0)
        {
            stateMachine.State = State.Wander;
            zapDuration = 3f;
        }
        if (zapTween != null)
            return;
    }

    public override void _Process(double delta)
    {
        if (!AIIsOn)
            return;
        stateMachine.Update(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();
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

    public void OnNoiseHeard(Node3D noiseSource, float dB, string noise)
    {
        stateMachine.State = State.Electric;
        zapTarget = noiseSource;
    }

    void ITakeDamage.OnDamageTaken(float amount, Vector3 knockback, Node3D source)
    {
        stateMachine.State = State.Electric;
        zapTarget = source;
    }
}
