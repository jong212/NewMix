using System.Collections;
using Unity.VisualScripting;
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

    }

    public override void Update()
    {
        base.Update();  // 기본 Update 호출
  
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if(enemy.closestPlayerTransform == null) stateMachine.ChangeState(enemy.idleState, ((int)EnemyStateID.Idle));

              
        if(enemy.CheckAgroDistance())
        {
            stateTimer = enemy.battleTime;
            Debug.Log("GoodAttack");
        }  
        else
        {
            if(stateTimer < 0 || enemy.GetHorizontalDistance(enemy.transform.position, enemy.closestPlayerTransform.position) > 10)
            {
                stateMachine.ChangeState(enemy.idleState, ((int)EnemyStateID.Idle));
                Debug.Log("PlayerOut");

            }
        }

        float distanceToPlayerX = Mathf.Abs(enemy.closestPlayerTransform.position.x - enemy.transform.position.x);
        if (distanceToPlayerX < 1.8f) return;
        Vector3 directionToPlayer = (enemy.closestPlayerTransform.position - enemy.transform.position).normalized;
        directionToPlayer.y = 0; // Y축을 무시하여 평면에서만 이동

        enemy.rb.velocity = directionToPlayer * enemy.moveSpeed;

    }
    public override void Exit()
    {
        base.Exit();
    }

    
}
