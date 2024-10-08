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
        Debug.Log("enter");
        stateTimer = enemy.moveTime;
        enemy.SetZeroVelocity();
        rb.isKinematic = false;
        enemy.SetRandomMoveDirection(); // 방향 1회
        enemy.UpdateRayDirections(); // Ray Arr
        enemy.RotateTowardsMoveDirection(); // 회전 
        enemy.OnEnterMoveState(); 



    }

    public override void Exit()
    {
        base.Exit();

    }

    public override void Update()
    {
        base.Update();
        Debug.Log("Update2");
        if (stateTimer < 0 || enemy.IsObstructed())
        {
            stateMachine.ChangeState(enemy.idleState);
        }
    }
    public override void FixedUpdate()
    {
        Debug.Log("Update3");
        base.FixedUpdate();
        enemy.Move();

    }
}
