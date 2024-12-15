using System;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

// MonsterManager에서 SpawnMonsterFromPool 를 통해 몬스터를 꺼내 Active true 하면 현재 스크립트의 Onenable이 실행 된다. 

public class MycoreStates : MycoreCommon
{
    public MyMonsterIdleState idleState { get; private set; }

    public MyMonsterStateMachine stateMachine { get; private set; }

    protected override void Awake()
    {
        base.Awake();
 
        idleState = new MyMonsterIdleState(this, stateMachine, "Idle", this);
  /*      moveState = new EnemyMoveState(this, stateMachine, "Move", this);
        battleState = new EnemyBattleState(this, stateMachine, "Battle", this);
        attackState = new EnemyAttackState(this, stateMachine, "Attack", this);
 */
    }
    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }
}
