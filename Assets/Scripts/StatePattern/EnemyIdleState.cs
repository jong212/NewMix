using System.Collections;
using UnityEngine;

public class EnemyIdleState : EnemyGroundedState
{
    public EnemyIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EnemyAi _enemy) : base(_enemyBase, _stateMachine, _animBoolName,_enemy)
    {
    }


    public override void Enter()
    {
        base.Enter();
        stateTimer = enemy.idleTime;
        enemyBase.SetZeroVelocity();
    }

    public override void Exit()
    {
        base.Exit();

    }

    public override void Update()
    {
        base.Update();

        if(stateTimer < 0)
        {
            stateMachine.ChangeState(enemy.moveState);
        }
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }
}
