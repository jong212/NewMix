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
        if (enemy.IsPlayerWithinRange())
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }
}
