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
        GameObject enemyObject = enemyBase.gameObject; // base 클래스에 있는 enemyBase를 참조
        GameObject.Destroy(enemyObject); // Destroy를 MonoBehaviour에서 호출
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
