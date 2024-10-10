using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 상태가 전환되면 Enemy 객체의 값은 초기화 된다. 
// 모든 상태클래스는 EnemyState를 상속받는다 
// 자식 클래스에서 base 하는 것을 감안한 로직을 작성할것 예를들면 idle 상태일때 idle 모션을 하기 위해 enemystate enter에서 enemybase.anim.setbool(animboolname,true)이런식으로
public class EnemyState
{
    // 각 상태 클래스에서 현재 상태를 알 수 있도록 
    protected EnemyStateMachine stateMachine;
    public Enemy enemyBase;
    protected Rigidbody rb;

    private string animBoolName;
    protected float stateTimer;
    protected bool triggerCalled;

    public EnemyState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName)
    {
        this.enemyBase = _enemyBase;
        this.stateMachine = _stateMachine;
        this.animBoolName = _animBoolName;
    }

    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;
    }


    public virtual void Enter()
    {
        //triggerCalled = false;
        rb = enemyBase.rb; //각 상태에서 enemyBase.rb 이렇게 길게 쓰기 귀찮아서 rb로 사용할 수 있도록 처리
        enemyBase.anim.SetBool(animBoolName, true);


    }
    public virtual void FixedUpdate()
    {

    }

    public virtual void Exit()
    {
        enemyBase.anim.SetBool(animBoolName, false);
       // enemyBase.AssignLastAnimName(animBoolName);
    }

    public virtual void AnimationFinishTrigger()
    {
        triggerCalled = true;
    }
}
