using Fusion;
using Fusion.Addons.SimpleKCC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

using static Character;

public class Mentity : MycoreNetwork
{
    public event Action OnStatsChanged;

    public Character _player { get; set; }
    public Sprite prifileImg {  get; set; }
    public MAi _mai { get; set; }
    public int spawnidx {get;set;}
    public string _playerNickname { get; private set; }
    public float _attackCooldown = 3.0f; // 공격 쿨타임 (3초)
    public float _lastAttackTime = -3.0f; // 마지막 공격 시간 (게임 시작 시 바로 공격 가능하도록 초기화)
    public bool _noAttack = false;
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
    public virtual bool CheckAgroDistance()
    {
        if (mymonsterMovement.Pathfinding.target != null)
        {
            float distanceToPlayer = GetHorizontalDistance(transform.position, mymonsterMovement.Pathfinding.target.transform.position);
            if (distanceToPlayer <= 10)
            {
                return true;
            }
        }
        return false;
    }
    public void InitHpUpdate()
    {
        if (OnStatsChanged != null)
        {
            OnStatsChanged?.Invoke();
        }
    }
    public float GetHorizontalDistance(Vector3 pos1, Vector3 pos2)
    {
        // Y값을 0으로 설정하여 XZ 평면의 거리만 계산
        pos1.y = 0;
        pos2.y = 0;
        return Vector3.Distance(pos1, pos2);
    }
    public void AttackingCheck(int isAttacking)
    {
        bool isAnimationStart = (isAttacking == 1); // TRUE 공격중
        IsAttack = (isAnimationStart) ? true : false;
    }
    public void PerformAttack()
    {
        if (MyMonsterMovement.Pathfinding.target != null)
        {
            if (Time.time - _lastAttackTime < _attackCooldown)
            {
                _noAttack = true;
                Debug.Log("쿨타임 중입니다. 다음 공격까지 대기하세요.");
                return; // 쿨타임이 끝나지 않았으므로 공격 실행하지 않음
            }
            _noAttack = false;
            Enemy targetMonster = MyMonsterMovement.Pathfinding.target.GetComponent<Enemy>();
            if (targetMonster != null)
            {
                _lastAttackTime = Time.time;
                AttackRpc(targetMonster, Atk);
            }
           else
            {
                Debug.LogWarning("Pathfinding.target에 Entity 컴포넌트가 없습니다.");
            }
        }
        else
        {
            Debug.Log("Pathfinding.target이 설정되지 않았습니다.");
        }
    }
    public void AttackRpc(Enemy targetMonster, float finalAtk)
    {
        NetworkObject nObject = targetMonster.GetComponent<NetworkObject>();
        _mai.stateMachine.ChangeState(_mai.attackState);
        if (targetMonster.NetworkedHealth <= 0)
        {
            MyMonsterMovement.path.Clear();
            MyMonsterMovement.Pathfinding.target = null;
            return;
        }
        else if (targetMonster.NetworkedHealth - finalAtk <= 0)
        {
            targetMonster.DealDamageRpc(finalAtk);
            MyMonsterMovement.path.Clear();
            MyMonsterMovement.Pathfinding.target = null;
            PlayAttackAnimationRpc(finalAtk, nObject);
            int tempIdx = 0;
            foreach (var item in targetMonster.DropItemPercent)
            {
                int randomValue = Random.Range(0, 100); // 0~99 사이의 랜덤 값 생성
                if (randomValue < item) // 드랍 됨
                {
                    RpcItemDropMethod(nObject, targetMonster.MonsterId, _playerNickname, targetMonster.DropItemIdx[tempIdx]);
                }
                else
                {

                }
                tempIdx++;

            }
        }
        else
        {
            targetMonster.DealDamageRpc(finalAtk);
            PlayAttackAnimationRpc(finalAtk, nObject);
        }
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void PlayAttackAnimationRpc(float damage, NetworkObject trs)
    {
        if (anim != null)
        {
            if (Object.HasStateAuthority)
            {
            _player.currentState = chrState.AttackStop;
            }
            StaticManager.UI.DamagePoolUI.ShowDamage(trs, damage.ToString());

        }
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcItemDropMethod(NetworkObject trs, float monsterid, string Nickname, int dropIdx)
    {
        StaticManager.UI.DropItemPoolManager.ShowDropItem(trs, monsterid, Nickname, dropIdx);
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
    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
        _player = StaticManager.Instance.UniquePlayer;
        _playerNickname = _player.Nickname.ToString();
            
        }
        var kccColliderTransform = transform.Find("KCCCollider");
        if (kccColliderTransform != null)
        {
            var capsuleCollider = kccColliderTransform.GetComponent<CapsuleCollider>();
            if (capsuleCollider != null)
            {
                capsuleCollider.isTrigger = false;
                // 추가적인 Collider 설정이 필요하면 여기에 작성
            }
        }
        
    }
    protected override void OnNicknameChanged()
    {
        StaticManager.UI.MainUI.MonUIList[spawnidx].init(this);
        StaticManager.UI.MainUI.MonUIList[spawnidx].SetProfileImg(prifileImg);
    }
    protected override void Update()
    {
        base.Update();
    }     
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    } 
    public void Despawn()
    {
        Runner.Despawn(Object);
    }
}
