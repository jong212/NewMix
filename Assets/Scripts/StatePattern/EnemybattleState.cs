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
        enemy.moveSpeed = 10f;

    }

    public override void Update()
    {
        base.Update();  // 기본 Update 호출
  
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 다른 플레이어가 나가는 경우 null 발생 위험이 있어 idlstate로 상태 변경
        if (enemy.closestPlayerTransform == null) 
        {
            stateMachine.ChangeState(enemy.idleState);
        }

        float xVelocity = enemy.rb.velocity.magnitude;
        Debug.Log(xVelocity);

        // 임계값(threshold)을 정해, 그 값보다 작으면 속도를 0으로 처리 (이거 안 하면 rb의 물리값 때문에 블렌드 트리에서 떨림현상이 발생함)
        float velocityThreshold = 0.01f; // 움직임이 없다고 간주할 최소 값
        if (xVelocity < velocityThreshold)
        {
            xVelocity = 0;
        }

        enemy.anim.SetFloat("xVelocity", xVelocity);

        if (enemy.CheckAgroDistance())
        {
            stateTimer = enemy.battleTime;
            Debug.Log("GoodAttack");
        }  
        else
        {
            if(enemy.closestPlayerTransform && (stateTimer < 0 || enemy.GetHorizontalDistance(enemy.transform.position, enemy.closestPlayerTransform.position) > 10))
            {
                stateMachine.ChangeState(enemy.idleState);
                Debug.Log("PlayerOut");
            }
        }
        if (enemy.closestPlayerTransform == null)
        {
            stateMachine.ChangeState(enemy.idleState);

        }

        // 플레이어와 너무 가까워지면 속도 조절
        if (enemy.closestPlayerTransform != null)
        {
            float distanceToPlayer = enemy.GetHorizontalDistance(enemy.transform.position, enemy.closestPlayerTransform.position);

            if (distanceToPlayer < 1.9f)
            {
                // 너무 가까워지면 천천히 속도를 줄여서 정지하도록 함
                float speedFactor = Mathf.Clamp01(distanceToPlayer / 1.9f); // 거리에 따라 속도 감소 비율 설정
                enemy.rb.velocity = enemy.rb.velocity * speedFactor; // 속도를 줄이도록 처리
                if (distanceToPlayer <1.5f) // 충분히 가까워지면 완전히 정지
                {
                    enemy.SetZeroVelocity();
                }
                return;
            }

            // 적이 플레이어를 추격하도록 호출
            enemy.TargetMoveMonster();
        }

    }
    public override void Exit()
    {
        base.Exit();
    }

    
}
