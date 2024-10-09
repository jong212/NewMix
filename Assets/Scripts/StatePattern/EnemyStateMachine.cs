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
        // ���� ���³� �� ���°� null�̸� ó������ ����
        if (currentState == null || _newState == null)
        {
            Debug.LogWarning("State change aborted: currentState or newState is null.");
            return;
        }

        // enemyBase�� ��ȿ���� Ȯ�� (���� ���ĸ� ���)
        if (currentState.enemyBase == null || !currentState.enemyBase.Object.IsValid)
        {
            Debug.LogWarning("State change aborted: enemyBase is null or has been despawned.");
            return;
        }

        Debug.Log("Attempting to change state. Current state: " + currentState);
        if (currentState.enemyBase.Object.HasStateAuthority)
        {
            Debug.Log("Has state authority. Exiting current state.");
            currentState.Exit();
            currentState = _newState;
            currentState.Enter();

            Debug.Log($"Setting NetworkedStateId to: {stateId}");
            currentState.enemyBase.NetworkedStateId = stateId;
        }
    }
}

