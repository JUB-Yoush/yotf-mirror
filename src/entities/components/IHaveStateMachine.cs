using System;
using System.Runtime.CompilerServices;

namespace Yotf;

//TODO (j) I don't think this is nessicary
public interface IHaveStateMachine<TIndex>
    where TIndex : unmanaged, Enum
{
    StateMachine<TIndex> stateMachine { get; }
    public void SetState(TIndex newState) => stateMachine.State = newState;
}
