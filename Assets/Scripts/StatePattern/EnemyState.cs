using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// EnemyState는 몬스터 각자의 EnemyState 이다.
// 모든 상태 클래스들은 EnemyState를 상속받게끔 한다
// 각각의 상태클래스는 부모인 EnemyState에게 stateMachine 등등 필요한 정보를 저장해둔다 필요할 때 꺼내 쓸 수 있도록
// 필요할 때 꺼내쓸 수 있는 값이 아니라 공용의 변수들을 설정하기 좋음 stateTimer 이런것들 실제 값은 다른데서 가져와야 한다 
// 자식 클래스에서 base 하는 것을 감안한 로직이 작성되어야 함 예를들면 idle 상태일때 idle 모션을 하기 위해 enemystate enter에서 enemybase.anim.setbool(animboolname,true)이런식으로
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
       // enemyBase.anim.SetBool(animBoolName, true);

    }
    public virtual void FixedUpdate()
    {

    }

    public virtual void Exit()
    {
        //enemyBase.anim.SetBool(animBoolName, false);
       // enemyBase.AssignLastAnimName(animBoolName);
    }

    public virtual void AnimationFinishTrigger()
    {
        triggerCalled = true;
    }
}
