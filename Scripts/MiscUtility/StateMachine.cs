using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine<T> : ScriptableObject where T : MovingObject
{
    internal abstract void OnStateEnter(T obj);
    internal abstract void UpdateState();
    internal abstract void OnStateExit(StateMachine<T> nextState);
}
