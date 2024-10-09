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

    public void ChangeState(EnemyState _newState, int stateId)
    {
        // Check if currentState or newState is null
        if (currentState == null)
        {
            currentState = _newState;
        }

        if (_newState == null)
        {
            Debug.LogError("New state is null. Cannot change to the new state.");
            return;
        }

        Debug.Log("현재상태 : " + currentState);
        if (currentState.enemyBase.Object.HasStateAuthority)
        {

            currentState.Exit();
            currentState = _newState;
            currentState.Enter();
            currentState.enemyBase.NetworkedStateId = stateId;
        }

    }
}
