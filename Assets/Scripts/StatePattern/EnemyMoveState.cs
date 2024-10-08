using System.Collections;
using UnityEngine;

public class EnemyMoveState : EnemyGroundedState
{
    public EnemyMoveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EnemyAi _enemy) : base(_enemyBase, _stateMachine, _animBoolName,_enemy)
    {
    }


    public override void Enter()
    {
        base.Enter();
        stateTimer = enemy.moveTime;
        rb.isKinematic = false;
       enemy.SetRandomMoveDirection();



    }

    public override void Exit()
    {
        base.Exit();

    }

    public override void Update()
    {
        base.Update();
        if (stateTimer < 0 || enemy.IsObstructed())
        {
            stateMachine.ChangeState(enemy.idleState);
        }
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        enemy.Move();

    }
}
