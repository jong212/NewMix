using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine
{

    public EnemyState currentState { get; private set; }

    public void Initialize(EnemyState _startState)
    {

        currentState = _startState;
        currentState.Enter();
    }

    public void ChangeState(EnemyState _newState)
    {
        if (currentState.enemyBase.Object.HasStateAuthority)
        {
            currentState.Exit();
            currentState = _newState;
            currentState.Enter();
        }

    }
}
