using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroundedState : EnemyState
{
    protected EnemyAi enemy;
    public EnemyGroundedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName,EnemyAi _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

    }
    public override void FixedUpdate()
    {
        
        if (enemy.GetClosestPlayerWithinRange() != null)
        {
            // ���� ����� �÷��̾ ������ �� ó��
            stateMachine.ChangeState(enemy.battleState, ((int)EnemyStateID.Battle));
        }
            enemy.DrawRayPlayerDirection();

        base.FixedUpdate();
    }
}
