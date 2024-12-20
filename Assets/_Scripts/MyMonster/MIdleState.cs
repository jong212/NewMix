using System.Collections;
using UnityEngine;

public class MIdleState : MyMonsterGroundedState
{

    public Character _player { get; private set; }
    public MIdleState(MAi _enemyBase, MStateMachine _stateMachine, string _animBoolName, Mentity _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
    {
    }


    public override void Enter()
    {
        base.Enter();
        _player = StaticManager.Instance.UniquePlayer;
    }

    public override void Exit()
    {
        base.Exit();

    }

    public override void Update()
    {
        base.Update();
      if( _player != null && _player.PlayerMovement.path != null)
        {            
            if (_player.currentState == Character.chrState.TargetMove)
            {
                Debug.Log("targetIng");
            }
            else
            {
                Debug.Log("Notarget");
            }
        }
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }
}
