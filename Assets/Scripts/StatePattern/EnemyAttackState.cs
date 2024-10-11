using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyAttackState : EnemyState
{
    EnemyAi enemy;
    public EnemyAttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EnemyAi _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }


    public override void Enter()
    {
        base.Enter();

    }

    public override void Update()
    {
        base.Update();  // 기본 Update 호출

    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        enemy.SetZeroVelocity();

        if (triggerCalled)
            stateMachine.ChangeState(enemy.battleState);
    }
  
    public override void Exit()
    {
        base.Exit();
        enemy.lastTimeAttacked = Time.time;

    }


}
