using System;
using UnityEngine;
using Random = UnityEngine.Random;

// MonsterManager에서 SpawnMonsterFromPool 를 통해 몬스터를 꺼내 Active true 하면 현재 스크립트의 Onenable이 실행 된다. 

public class EnemyAi : Enemy
{
    #region States

    public EnemyIdleState idleState { get; private set; }
    public EnemyMoveState moveState { get; private set; }
    public EnemyBattleState battleState { get; private set; }
    public EnemyAttackState attackState { get; private set; }

    #endregion

    protected override void Awake()
    {
        base.Awake();
        
        idleState = new EnemyIdleState(this, stateMachine, "Idle", this);
        moveState = new EnemyMoveState(this, stateMachine, "Move", this);
        battleState = new EnemyBattleState(this, stateMachine, "Battle", this);
        attackState = new EnemyAttackState(this, stateMachine, "Attack", this);

    }
    protected override void Start()
    {
        base.Start();
            stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
         //TEMPHIDE// Debug.Log($"[몬스터 현재 상태 : {stateMachine.currentState.ToString()}]");

        if (Object.HasStateAuthority)
        {
            base.Update();
        }
    }

    protected override void FixedUpdate()
    {
        if (Object.HasStateAuthority)
        {
            base.FixedUpdate();
        }
    }

    public override void DealDamageRpc(float damage)
    {
        base.DealDamageRpc(damage);
    }
    public override void OnTriggerEnter(Collider col)
    {
        base.OnTriggerEnter(col);   // 부모 클래스에서 이미 플레이어 감지 처리를 했음
        if (target != null)         // 추가적인 동작만 수행 (중복 제거)
        {
            Debug.Log(col.tag.ToString());
            // EnemyAI 고유의 추가 행동 작성
        }
    }
    public override void Die()
    {
        base.Die();
    }

    // 몬스터 오브젝트 풀에서 꺼내 사용할 때 상태 초기화 하기 위해 Idlestate로 변경
    // ischeck는 씬이 처음 열릴 때 초기화 하는 Awake 단에서 Onebable이 오류나서 첫 스폰시점에서는 동작 안 하게 하기 위해
    public bool ischeck;
    public override void OnEnable()
    {
     
        if (ischeck)                                // 두 번째 호출부터 실행
        {
            stateMachine.Initialize(idleState);
        }
        ischeck = true;                             // 처음 호출 후 ischeck를 true로 설정
    }
    public override void OnDisable()
    {
        base.OnDisable();
    }
}
