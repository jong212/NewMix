using System.Collections;
using UnityEngine;

public class MIdleState : MyMonsterGroundedState
{

    public MIdleState(MAi _enemyBase, MStateMachine _stateMachine, string _animBoolName, Mentity _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
    {
    }


    public override void Enter()
    {
        base.Enter();
        //_enemy.mymonsterMovement.CanMove = false;

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
         if (_enemy._player != null && _enemy._player.PlayerMovement.path != null )
        {
         /* if (_enemy._player.currentState == Character.chrState.TargetMove)
            {
                enemyBase.stateMachine.ChangeState(enemyBase.moveState);
            } else if (_enemy._player.currentState == Character.chrState.Attack && !_enemy._noAttack)
            {
                enemyBase.stateMachine.ChangeState(enemyBase.attackState);

            }*/
            /*else if (_enemy._player.currentState == Character.chrState.Attack && _enemy.CheckAgroDistance() && !_enemy.IsAttack)
            {
                enemyBase.stateMachine.ChangeState(enemyBase.attackState);
            }*/
        } 
    }
}
