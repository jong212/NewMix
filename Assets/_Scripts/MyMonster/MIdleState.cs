using System.Collections;
using UnityEngine;

public class MIdleState : MyMonsterGroundedState
{
    
    public MIdleState(MAi _enemyBase, MStateMachine _stateMachine, string _animBoolName, Mentity _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
    {
    }


    public override void Enter()
    {
        base.Enter();
      /*  stateTimer = enemy.idleTime;
        enemyBase.SetZeroVelocity();*/
    }

    public override void Exit()
    {
        base.Exit();

    }

    public override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            enemy.Runner.Despawn(enemy.Object); // NetworkObject Á¦°Å
        }
        if (stateTimer < 0)
        {
/*            stateMachine.ChangeState(enemy.moveState);
*/        }
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }
}
