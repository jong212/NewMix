using System.Collections;
using UnityEngine;

public class EnemyIdleState : EnemyState
{
    private EnemyAi enemy;
    public EnemyIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EnemyAi _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
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

        enemy.lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();
     //   Debug.Log("test");
        enemy.SetZeroVelocity();
     
    }
}
