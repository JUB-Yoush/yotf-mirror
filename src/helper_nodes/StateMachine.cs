using System;
using System.Runtime.CompilerServices;
using Godot;

namespace Yotf;

//modified from: https://github.com/EXOK/Celeste64/blob/main/Source/Helpers/StateMachine.cs
public sealed unsafe class StateMachine<TIndex>
    where TIndex : unmanaged, Enum
{
    private static readonly int StateCount = Enum.GetValues<TIndex>().Length;

    //private static int StateToIndex(TIndex state) => *(int*)(&state);

    private static int StateToIndex(TIndex state) => Unsafe.As<TIndex, int>(ref state);

    private readonly Action<float>?[] update = new Action<float>[StateCount];
    private readonly Action?[] enter = new Action[StateCount];
    private readonly Action?[] exit = new Action[StateCount];

    private TIndex? state;

    public TIndex? State
    {
        get => state;
        set
        {
            PreviousState = state;
            state = value;
            if (PreviousState.HasValue)
                exit[StateToIndex(PreviousState.Value)]?.Invoke();
            if (state.HasValue)
            {
                enter[StateToIndex(state.Value)]?.Invoke();
            }
        }
    }
    public TIndex? PreviousState { get; private set; }

    public void AddState(
        TIndex state,
        Action<float>? update,
        Action? enter = null,
        Action? exit = null
    )
    {
        int index = StateToIndex(state);
        this.update[index] = update;
        this.enter[index] = enter;
        this.exit[index] = exit;
    }

    public void Update(double deltaTime)
    {
        if (state.HasValue)
            update[StateToIndex(state.Value)]?.Invoke((float)deltaTime);
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
