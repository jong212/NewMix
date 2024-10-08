using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class EnemyAi : Enemy
{

    [Header("Archer spisifc info")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float arrowSpeed;
    [SerializeField] private float arrowDamage;

    
    public Vector2 jumpVelocity;
    public float jumpCooldown;
    public float safeDistance; // how close palyer should be to trigger jump on battle state
    [HideInInspector] public float lastTimeJumped;

    [Header("Additional collision check")]
    [SerializeField] private Transform groundBehindCheck;
    [SerializeField] private Vector2 groundBehindCheckSize;

    //테스트
    #region States

    public EnemyIdleState idleState { get; private set; }
    public EnemyMoveState moveState { get; private set; }
    public EnemyBattleState battleState { get; private set; }

    #endregion


    protected override void Awake()
    {
        base.Awake();
        idleState = new EnemyIdleState(this, stateMachine, "Idle", this);
        moveState = new EnemyMoveState(this, stateMachine, "Move", this);
        battleState = new EnemyBattleState(this, stateMachine, "battleState", this);

    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }


    /*public override bool CanBeStunned()
    {
        if (base.CanBeStunned())
        {
            //stateMachine.ChangeState(stunnedState);
            return true;
        }

        return false;
    }*/

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

    public override void AnimationSpecialAttackTrigger()
    {
        GameObject newArrow = Instantiate(arrowPrefab, attackCheck.position, Quaternion.identity);
        //newArrow.GetComponent<Arrow_Controller>().SetupArrow(arrowSpeed * facingDir, stats);
    }

/*    public bool GroundBehind() => Physics2D.BoxCast(groundBehindCheck.position, groundBehindCheckSize, 0, Vector2.zero, 0, whatIsGround);
    public bool WallBehind() => Physics2D.Raycast(wallCheck.position, Vector2.right * -facingDir, wallCheckDistance + 2, whatIsGround);*/

 /*   protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.DrawWireCube(groundBehindCheck.position, groundBehindCheckSize);
    }*/
}
