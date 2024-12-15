using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MyMonsterStateMachine
{
    public MyMonsterState currentState { get; private set; }

    public void Initialize(MyMonsterState _startState)
    {
        currentState = _startState;
        currentState.Enter();
    }

    public void ChangeState(MyMonsterState _newState)
    {
        if (currentState.enemyBase == null || !currentState.enemyBase.Object.IsValid || !currentState.enemyBase.Object.HasStateAuthority || currentState == null || _newState == null) return;

        currentState.Exit();
        currentState = _newState;
        currentState.Enter();
    }
}

