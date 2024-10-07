using System.Collections;
using UnityEngine;

public class EnemyBattleState : EnemyState
{
    EnemyAi enemy;
    public EnemyBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, EnemyAi _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }


    public override void Enter()
    {
        base.Enter();        
        enemyBase.Runner.Despawn(enemyBase.Object);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }
}
