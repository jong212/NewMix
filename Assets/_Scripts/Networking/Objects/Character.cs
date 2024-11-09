using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class Character : NetworkBehaviour
{
    private bool _isInitialized = false;

    // Private Fields
    private VariableJoystick _joystick;
    private PlayerInput _prevInput;
    private WorldNickname _nicknameUI;
    private Vector2 _joystickInput;
    private bool _isMoveAble;
    private bool _isAttack;
    public bool IsAttack {
        get => _isAttack;
        set => _isAttack = value;
    }


    // Serialized Fields
    [field: SerializeField]
    public CharacterSpecs Specs { get; private set; }

    [SerializeField] private List<Transform> itemList;

    [SerializeField] private SimpleKCC _kcc;
    [SerializeField] private Transform _uiPoint;
    [SerializeField] private Animator _anim;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private MouseManager _mouseManager;

    [SerializeField] private float attackRange = 2.0f;     // 공격 범위
    [SerializeField] private LayerMask monsterLayerMask;  // 몬스터 레이어 마스크
    // Networked Properties
    [Networked, OnChangedRender(nameof(OnNicknameChanged))]
    public NetworkString<_16> Nickname { get; set; }

    [Networked, Capacity(2), OnChangedRender(nameof(OnSetitemList))]
    public NetworkArray<int> setItemIndexs { get; }


    [Networked] public int Level { get; set; }
    [Networked] public int Attack { get; set; }
    [Networked] public int Health { get; set; }
    [Networked] public int MissChance { get; set; }
    [Networked] public bool WaitingForAuthority { get; set; }
    [Networked] public Item HeldItem { get; set; }

    // Unity Callbacks
    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            InitializeJoystick();
            InitUI();
            InitPlayer();
            ModifyKCCCollider();
        }
        if (Object.HasStateAuthority)
        {
            Debug.Log($"[Client {Runner.LocalPlayer.PlayerId}] ???444");
            InitItem(); // State Authority에서 네트워크 속성 초기값 설정
        }
        
        //OnSetitemList();
    }

    private void InitializeJoystick()
    {
        _joystick = FindObjectOfType<VariableJoystick>();
    }
    private void InitUI() {
        StaticManager.UI.CommonOpen(UIType.BtnAttack, StaticManager.UI.MainUI.Layout_BottomRight, true, ChangeMoveProperty);
    }
    private void SetupCameraFollow()
    {
        var cameraFollow = FindObjectOfType<IsometricCameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.target = this.transform;
        }
        else
        {
            Debug.LogWarning("IsometricCameraFollow component not found in the scene.");
        }
    }

    private void SetInitialNickname()
    {
        Nickname = BackendGameData.Instance.NickName;
    }

    private void InitPlayer()
    {
        SetupCameraFollow();
        SetInitialNickname();
        InitStat();
        InitializeNicknameUI();
    }
    private void InitStat()
    {
        var userData = BackendGameData.Instance.userData;
        Level = userData.Level;
        Attack = userData.Atk;
        Health = userData.Hp;
        MissChance = userData.Miss;
        Debug.Log($"Player spawned with Level: {Level}, Attack: {Attack}, Health: {Health}");
    }
    private void InitItem() // 이 함수에다가는 캐싱 된 아이템 인덱스 넣고 onrender 통해타는 함수에서 장착 해제 하는 코드 작성 ㄱ
    {
        List<int> playerItemsList = BackendGameData.Instance.userData.setPlayerItems;
        setItemIndexs.Clear();
        setItemIndexs.CopyFrom(playerItemsList, 0, playerItemsList.Count);

    }
    private void InitializeNicknameUI()
    {
        _nicknameUI = Instantiate(StaticManager.UI.WorldNickNameUI, StaticManager.Instance.WorldCanvas.transform);
        OnNicknameChanged();
    }

    private void ModifyKCCCollider()
    {
        var kccColliderTransform = transform.Find("KCCCollider");
        if (kccColliderTransform != null)
        {
            var capsuleCollider = kccColliderTransform.GetComponent<CapsuleCollider>();
            if (capsuleCollider != null)
            {
                capsuleCollider.isTrigger = false;
                // 추가적인 Collider 설정이 필요하면 여기에 작성
            }
            else
            {
                Debug.LogWarning("CapsuleCollider not found on KCCCollider object.");
            }
        }
        else
        {
            Debug.LogWarning("KCCCollider object not found as a child.");
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (Object.HasInputAuthority)
        {
            HandleMouseInput();
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _mouseManager.ClickCheck();
            if (_playerMovement.Pathfinding.target && _joystickInput.magnitude <= 0)
            {
                _isMoveAble = false;
            }
        }
    }

    public override void Render()
    {
        if (!_isInitialized && setItemIndexs.Length > 0)
        {
            _isInitialized = true;
            OnSetitemList();
        }

        float movementSpeed = _kcc.RealSpeed > 0 ? _kcc.RealSpeed / Specs.MovementSpeed : 0;
        _anim.SetFloat("Movement", movementSpeed);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority)
            return;

        ProcessMovement();
    }

    private void ProcessMovement()
    {
        if (IsAttack) return;
        if (_joystick != null)
        {
            _joystickInput = new Vector2(_joystick.Horizontal, _joystick.Vertical);
            if (_joystickInput.magnitude > 0)
            {
                _isMoveAble = true;
                MoveCharacter(_joystickInput);
            }
            else
            {
                HandleIdleMovement();
            }
        }
    }

    private void MoveCharacter(Vector2 input)
    {
        Vector3 moveDirection = new Vector3(input.x, 0, input.y);
        Vector3 finalMoveDirection = CalculateFinalMoveDirection(moveDirection);

        _kcc.Move(finalMoveDirection * Specs.MovementSpeed);

        if (finalMoveDirection.magnitude > 0)
        {
            float lookRotation = Mathf.Atan2(finalMoveDirection.x, finalMoveDirection.z) * Mathf.Rad2Deg;
            _kcc.SetLookRotation(0, lookRotation);
        }
    }

    private Vector3 CalculateFinalMoveDirection(Vector3 moveDirection)
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward.Normalize();
        cameraRight.Normalize();

        return cameraForward * moveDirection.z + cameraRight * moveDirection.x;
    }

    private void HandleIdleMovement()
    {
        if (_playerMovement.Pathfinding.target  && !_isMoveAble)
        {
            _playerMovement.Movement();
            return;
        }

        _kcc.Move(Vector3.zero);
    }
    public void PerformAttack()
    {
        if (_playerMovement.Pathfinding.target != null)
        {
            Entity targetMonster = _playerMovement.Pathfinding.target.GetComponent<Entity>();
            if (targetMonster != null)
            {
                AttackRpc(targetMonster);
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
    // Attack Mechanism
 
    public void AttackRpc(Entity targetMonster)
    {
        
        if(targetMonster.NetworkedHealth <= 0)
        {
            _playerMovement.path.Clear();
            _playerMovement.Pathfinding.target = null;
        } else
        {
        targetMonster.DealDamageRpc(10);
            PlayAttackAnimationRpc();
        }
        //PushMonster(targetMonster);
        
    }
    private void PushMonster(Entity monster)
    {
        Rigidbody monsterRb = monster.GetComponent<Rigidbody>();
        Vector3 pushDirection = (monster.transform.position - transform.position).normalized;
        float pushForce = 50f;

        if (monsterRb != null)
        {
            monsterRb.isKinematic = false;
            monsterRb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
        }
        else
        {
            monster.transform.position += pushDirection * 0.5f;
        }
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void PlayAttackAnimationRpc()
    {
        if (_anim != null)
        {
            _anim.SetTrigger("Attack");
        }
    }

    // Network Change Detection
    private void OnNicknameChanged()
    {
        if (_nicknameUI != null)
        {
            _nicknameUI.SetTarget(_uiPoint, Nickname.Value);
        }
    }
    private void OnSetitemList()
    {
        Debug.Log($"[Client {Runner.LocalPlayer.PlayerId}] OnSetitemList called for Character with InputAuthority {Object.InputAuthority.PlayerId}");

        StaticManager.DataSetManager.SetCharacterItem(setItemIndexs, itemList);
    }

    // Item Handling
    public void SetHeldItem(Item item)
    {
        if (item == null)
        {
            ReleaseHeldItem();
        }
        else
        {
            AcquireHeldItem(item);
        }
    }

    private void ReleaseHeldItem()
    {
        HeldItem = null;
        _kcc.RefreshChildColliders();
    }

    private void AcquireHeldItem(Item item)
    {
        var authHandler = item.GetComponentTopmost<AuthorityHandler>();
        if (authHandler.TryGetComponent(out Character _)) return;

        WaitingForAuthority = true;

        authHandler.RequestAuthority(
            onAuthorized: () =>
            {
                ConfigureHeldItem(item);
                WaitingForAuthority = false;
                HeldItem = item;
                HeldItem.transform.SetParent(Object.transform);
                _kcc.RefreshChildColliders();
            },
            onUnauthorized: () => WaitingForAuthority = false
        );
    }

    private void ConfigureHeldItem(Item item)
    {
        if (item.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }

        if (item.TryGetComponent(out ColliderGroup cg))
        {
            cg.CollidersEnabled = false;
        }

        if (item.TryGetComponent(out WorkSurface surf))
        {
            surf.ItemOnTop = null;
        }
    }

    // Movement Property Change
    private void ChangeMoveProperty()
    {
        _isMoveAble = false;
    }

    // 플레이어 공격 애니메이션 시작,종료 프레임 이벤트
    public void AttackingCheck(int isAttacking)
    {
        bool isAnimationStart = (isAttacking == 1);
        IsAttack = (isAnimationStart) ? true : false;
    }
}
