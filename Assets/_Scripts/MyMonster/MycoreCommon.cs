using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MycoreCommon : MycoreNetwork
{
    public Animator anim { get; private set; } //다른 스크립트에서 entity.anim으로 애니메이터에 접근하여 애니메이션 상태를 확인할 수 있지만, 애니메이터를 변경할 수는 없다.
    public Rigidbody rb { get; private set; } // 엔티티에서 게터세터 사용으로 외부수정을 제한했다 만약 Enemy에서 rb = GetComponent<Rigidbody>(); 이런코드 쓰면 오류난다 하지만 rb.verocity 값 설정은 가능하다 재정의만 불가능
    public SpriteRenderer sr { get; private set; }
    public CapsuleCollider cd { get; private set; }
    protected virtual void Awake()
    {
        base.Awake();
    }
    protected virtual void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
    }
}
