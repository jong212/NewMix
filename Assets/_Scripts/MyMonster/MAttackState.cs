using System.Collections;
using UnityEngine;

public class MAttackState : MyMonsterGroundedState
{

    public MAttackState(MAi _enemyBase, MStateMachine _stateMachine, string _animBoolName, Mentity _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
    {
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
        enemyBase.stateMachine.ChangeState(enemyBase.idleState);

    }
}
