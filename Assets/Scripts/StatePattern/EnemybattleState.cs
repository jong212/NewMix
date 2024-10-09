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

        // Despawn 전에 확인
        if (enemyBase != null && enemyBase.Object != null && enemyBase.Object.IsValid)
        {
            Debug.Log("Despawning enemy...");
            enemyBase.Runner.Despawn(enemyBase.Object);  // 오브젝트 디스폰
            return;  // Despawn 이후 더 이상의 처리를 방지
        }
    }

    public override void Update()
    {
        // Despawn된 이후로 Update가 호출되지 않도록 방어 코드 추가
        if (enemyBase == null || !enemyBase.Object.IsValid)
        {
            Debug.LogWarning("Enemy has been despawned, skipping Update.");
            return;  // Despawn된 후라면 더 이상의 처리를 하지 않음
        }

        base.Update();  // 기본 Update 호출
    }
    public override void Exit()
    {
        base.Exit();
    }

    
}
