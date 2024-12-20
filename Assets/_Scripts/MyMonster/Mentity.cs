using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Mentity : MycoreNetwork
{
    public Animator anim { get; private set; } //다른 스크립트에서 entity.anim으로 애니메이터에 접근하여 애니메이션 상태를 확인할 수 있지만, 애니메이터를 변경할 수는 없다.
    public Rigidbody rb { get; private set; } // 엔티티에서 게터세터 사용으로 외부수정을 제한했다 만약 Enemy에서 rb = GetComponent<Rigidbody>(); 이런코드 쓰면 오류난다 하지만 rb.verocity 값 설정은 가능하다 재정의만 불가능
    public SpriteRenderer sr { get; private set; }
    public SimpleKCC kcc { get; private set; }
    public CapsuleCollider cd { get; private set; }
    [SerializeField] MyMonsterMovement MyMonsterMovement;
    public MyMonsterMovement mymonsterMovement{ get => MyMonsterMovement; }
    private bool _isAttack;

    public bool IsAttack
    {
        get => _isAttack;
        set => _isAttack = value;
    }
    public void AttackingCheck(int isAttacking)
    {
        bool isAnimationStart = (isAttacking == 1); // TRUE 공격중
        IsAttack = (isAnimationStart) ? true : false;
    }
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        kcc = GetComponentInChildren<SimpleKCC>();
        rb = GetComponent<Rigidbody>();
    }
    protected override void Update()
    {
        base.Update();
    }     
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    } 
}
