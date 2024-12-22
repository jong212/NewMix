using System.Collections;
using UnityEngine;

public class MMoveState : MyMonsterGroundedState
{

    public MMoveState(MAi _enemyBase, MStateMachine _stateMachine, string _animBoolName, Mentity _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
    {
    }


    public override void Enter()
    {
        base.Enter();
        //_enemy.mymonsterMovement.CanMove = true;

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
        if (_enemy._player.currentState == Character.chrState.AttackStop)
        {
            enemyBase.stateMachine.ChangeState(enemyBase.idleState);
        }
    }
}
