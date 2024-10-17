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

        // 다른 참가자 나가는 경우 대비 Null인 경우 상태 변경
        if (enemy.closestPlayerTransform == null) stateMachine.ChangeState(enemy.idleState);

        // 임계값(threshold)을 정해, 그 값보다 작으면 속도를 0으로 처리 (이거 안 하면 rb의 물리값 때문에 블렌드 트리에서 떨림현상이 발생함)    
        float xVelocity = enemy.rb.velocity.magnitude;        
        float velocityThreshold = 0.01f; 
        if (xVelocity < velocityThreshold) xVelocity = 0;
        enemy.anim.SetFloat("xVelocity", xVelocity);

        // 어그로 영역 이내로 들어왔는지 체크
        if (enemy.CheckAgroDistance())
        {
            stateTimer = enemy.battleTime;
            if (enemy.GetHorizontalDistance(enemy.transform.position, enemy.closestPlayerTransform.position) < enemy.attackDistance)
            {
                if (CanAttack())
                {
                    stateMachine.ChangeState(enemy.attackState);

                }
            }
            
                Debug.Log("GoodAttack");


        }  
        else
        {
            if(enemy.closestPlayerTransform && (stateTimer < 0 || enemy.GetHorizontalDistance(enemy.transform.position, enemy.closestPlayerTransform.position) > 10))
            {
                stateMachine.ChangeState(enemy.idleState);
            }
        }

        // 다른 참가자가 나가는 경우를 대비한 Null 처리
        if (enemy.closestPlayerTransform == null)
        {
            stateMachine.ChangeState(enemy.idleState);
        }

        // 플레이어와 너무 가까워지면 속도 조절
        if (enemy.closestPlayerTransform != null)
        {
            float distanceToPlayer = enemy.GetHorizontalDistance(enemy.transform.position, enemy.closestPlayerTransform.position);

            if (distanceToPlayer < 3)
            {
                // 너무 가까워지면 천천히 속도를 줄여서 정지하도록 함
                float speedFactor = Mathf.Clamp01(distanceToPlayer / 2); // 거리에 따라 속도 감소 비율 설정
                enemy.rb.velocity = enemy.rb.velocity * speedFactor;     // 속도를 줄이도록 처리
                if (distanceToPlayer <2.5f)                              // 충분히 가까워지면 완전히 정지
                {
                    enemy.SetZeroVelocity();
                }
                return;
            }

            // 타겟 방향으로 이동 및 회전
            enemy.TargetMoveMonster();
        }
    }
    private bool CanAttack()
    {
        if (Time.time >= enemy.lastTimeAttacked + enemy.attackCooldown)
        {
            enemy.lastTimeAttacked = Time.time;
            return true;
        }

        return false;
    }
    public override void Exit()
    {
        base.Exit();
    }

    
}
