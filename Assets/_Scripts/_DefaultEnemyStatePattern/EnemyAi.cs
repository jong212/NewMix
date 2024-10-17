using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

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
        Debug.Log("overrideTest");
    }
    public override void OnTriggerEnter(Collider col)
    {
        base.OnTriggerEnter(col); // 부모 클래스에서 이미 플레이어 감지 처리를 했음

        // 추가적인 동작만 수행 (중복 제거)
        if (target != null)
        {
            Debug.Log(col.tag.ToString());
            // EnemyAI 고유의 추가 행동 작성
        }
    }
    public override void Die()
    {
        base.Die();
        //stateMachine.ChangeState(deadState);

    }


}
