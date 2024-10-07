using System.Collections;
using UnityEngine;

public class EnemyMoveState : EnemyState
{
    private EnemyAi enemy;
    public EnemyMoveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EnemyAi _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }


    public override void Enter()
    {
        base.Enter();
        stateTimer = enemy.moveTime;
        rb.isKinematic = false;
        enemyBase.SetRandomMoveDirection();

    }

    public override void Exit()
    {
        base.Exit();

        enemy.lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();
        enemyBase.Move();
        if (stateTimer < 0)
        {
            stateMachine.ChangeState(enemy.idleState);
        }

       // enemy.SetZeroVelocity();
     
    }
}
