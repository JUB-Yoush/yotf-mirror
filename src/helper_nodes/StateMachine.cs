using System;
using System.Runtime.CompilerServices;
using Godot;

namespace Yotf;

//modified from: https://github.com/EXOK/Celeste64/blob/main/Source/Helpers/StateMachine.cs
public sealed unsafe class StateMachine<TIndex>
    where TIndex : unmanaged, Enum
{
    private static readonly int StateCount = Enum.GetValues<TIndex>().Length;

    private static int StateToIndex(TIndex state) => Unsafe.As<TIndex, int>(ref state);

    private readonly Action<float>?[] update = new Action<float>[StateCount];
    private readonly Action?[] enter = new Action[StateCount];
    private readonly Action?[] exit = new Action[StateCount];
    private readonly Func<CoEnumerator>?[] routine = new Func<CoEnumerator>[StateCount];

    private Routine running = new();

    public TIndex? State
    {
        get;
        set
        {
            PreviousState = field;
            field = value;
            running.Clear();
            if (PreviousState.HasValue)
                exit[StateToIndex(PreviousState.Value)]?.Invoke();
            if (field.HasValue)
            {
                enter[StateToIndex(field.Value)]?.Invoke();
                if (routine[StateToIndex(field.Value)] is { } rt) // type casting trick to assign to new var and check if not null
                    running.Run(rt());
            }
        }
    }
    public TIndex? PreviousState { get; private set; }

    public void AddState(
        TIndex state,
        Action<float>? update,
        Action? enter = null,
        Action? exit = null,
        Func<CoEnumerator>? routine = null
    )
    {
        int index = StateToIndex(state);
        this.update[index] = update;
        this.enter[index] = enter;
        this.exit[index] = exit;
        this.routine[index] = routine;
    }

    public void Update(double deltaTime)
    {
        if (State.HasValue)
            update[StateToIndex(State.Value)]?.Invoke((float)deltaTime);
        running.Update((float)deltaTime);
    }
}
//sample of how to use
// public partial class Unsafestatemachine : Node2D
// {
//     enum State
//     {
//         StateOne,
//         StateTwo,
//         StateThree,
//     }

//     private readonly StateMachine<State> stateMachine = new();

//     public override void _Ready()
//     {
//         stateMachine.AddState(State.StateOne, S1Update, null, null);
//         stateMachine.State = State.StateOne;
//     }

//     public override void _PhysicsProcess(double delta)
//     {
//         stateMachine.Update(delta);
//     }

//     public void S1Update(float delta)
//     {
//         Log.PrintLn("state 1 update");
//     }
// }
