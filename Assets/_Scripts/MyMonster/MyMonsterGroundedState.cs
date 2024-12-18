using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyMonsterGroundedState : Mstate
{
    protected Mentity enemy;
    public MyMonsterGroundedState(MAi _enemyBase, MStateMachine _stateMachine, string _animBoolName, Mentity _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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

      /*  if (enemy.GetClosestPlayerWithinRange() != null)
        {
            // 가장 가까운 플레이어가 존재할 때 처리
            stateMachine.ChangeState(enemy.battleState);
        }
        enemy.DrawRayPlayerDirection();*/

        base.FixedUpdate();
    }
}
