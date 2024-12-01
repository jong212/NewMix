using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

public class Character : NetworkBehaviour
{
    private bool _isInitialized = false;
    private VariableJoystick _joystick;
    private PlayerInput _prevInput;
    private WorldNickname _nicknameUI;
    private Vector2 _joystickInput;

    /// <summary>
    /// true => 조이스틱 값이 있을 때 <br></br>
    /// false => 조이스틱 값이 없을 때
    /// </summary>
    private bool _isMoveAble; 

    private bool _isAttack;
    /// <summary>
    /// true => 공격중 <br></br>
    /// false => 공격 안 하는 중
    /// </summary>
    public bool IsAttack {
        get => _isAttack;
        set => _isAttack = value;
    }

    [field: SerializeField]
    public CharacterSpecs Specs { get; private set; }
    public PlayerMovement PlayerMovement { get => _playerMovement; }

    [SerializeField] private List<Transform> itemList;
    [SerializeField] private List<Transform> itemParitsList;
    [SerializeField] private SimpleKCC _kcc;
    [SerializeField] private Transform _uiPoint;
    [SerializeField] private Animator _anim;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private MouseManager _mouseManager;
    [SerializeField] private float attackRange = 2.0f;     // 공격 범위
    [SerializeField] private LayerMask monsterLayerMask;  // 몬스터 레이어 마스크

    [Networked, OnChangedRender(nameof(OnNicknameChanged))] public NetworkString<_16> Nickname { get; set; }
    [Networked, Capacity(4), OnChangedRender(nameof(OnSetitemList))] public NetworkArray<int> setItemIndexs { get; }

    [Networked] public int Level { get; set; }
    [Networked] public int Attack { get; set; }
    [Networked] public int Health { get; set; }
    [Networked] public int Def { get; set; }
    [Networked] public int FinalAtk { get; set; }
    [Networked] public int FinalHP { get; set; }
    [Networked] public float FinalAtkSpeed { get; set; }
    [Networked] public float FinalMoveSpeed { get; set; }
    [Networked] public bool WaitingForAuthority { get; set; }
    [Networked] public Item HeldItem { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            InitializeJoystick();   // 조이스틱 On
            InitUI();               // 공격 버튼 On, 카메라 플레이어 Follow 하도록 초기화
            InitPlayer();           // 닉네임, 스텟 초기화
            ModifyKCCCollider();    // 플레이어 물리 관련 초기화
            InitItem();             // 플레이어 장비 장착 정보 네트워크 변수에 초기화 (다른 클라 동기화)
            StaticManager.UI.ContentsInventoryUI.gameObject.SetActive(true);
            StaticManager.Instance.CashUdata = BackendGameData.Instance.userData;
            StaticManager.Instance.UniquePlayer = this;
            CalculateStatUI();
            StaticManager.Instance.Stat += CalculateStatUI;
        } 
    }
    private void OnDisable()
    {
        StaticManager.Instance.Stat -= CalculateStatUI;
    }
    private void InitializeJoystick()
    {
        _joystick = FindObjectOfType<VariableJoystick>();
    }
    private void InitUI() {
        StaticManager.UI.CommonOpen(UIType.BtnAttack, StaticManager.UI.MainUI.Layout_BottomRight, true, ChangeMoveProperty);
        SetCameraUIFollowPlayer();
    }

    private void SetCameraUIFollowPlayer()
    {
        var cameraFollow = FindObjectOfType<IsometricCameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.target = this.transform;
        }
    }
    private void InitPlayer()
    {
        InitPlayerName();
        InitStat();
        InitializeNicknameUI();
    }
    private void InitPlayerName()
    {
        Nickname = BackendGameData.Instance.NickName;
    }

    private void InitStat()
    {
        var userData = BackendGameData.Instance.userData;
        Level = userData.Level;
        Attack = userData.Atk;
        Health = userData.Hp;
        Def = userData.Def;
        Debug.Log($"플레이어 오브젝트에 스텟 적용 Level: {Level}, Attack: {Attack}, Health: {Health}");
    }
    public void InitItem() 
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
        }
    }

    private void Update()
    {
        if (Object.HasInputAuthority)
        {
            HandleMouseInput();
         }
    }
    private void HandleMouseInput()
    {
        // UI위에 커서가 있을때 = ture/ 따라서 UI위에 커서가 없을때만 실행
        if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject() == false) {

            // 몬스터를 공격하는 방식이 2가지 있음 1. 몬스터 직접 클릭 2.우 하단 공격하기 버튼 이다.
            // 해당 IF문은 1번이며 몬스터를 클릭한다고 무조건 공격하는게 아님 
            // 예를들어 A 몬스터를 공격한다고 가정한다면
            // 1. A 몬스터 클릭 (타겟 A설정 됨 공격x)
            // 2. B 몬스터 클릭 (타겟 B로 바뀜 공격x)
            // 3. B 몬스터 클릭 (타겟 B상태에서 B 클릭한거라 _isMoveable = false 되서 공격하러감)
            // 이유는 공격버튼은 그냥 가장 가까이에 있는 몬스터한테 달려가면서 공격하도록 되어 있는데 몬스터를 터치한 이유는 그 몬스터의 레벨이나 정보를 보기 위함이라 타겟이 없으면 한 번 클릭해서 타겟 시켜놓고 또 몬스터를 클릭해서 공격하게끔 유도하기 위함
            // 그래서 previousTargetTransform 클릭 전 타겟 값을 TEMP 해놓고 있는 것이고 클릭했을 때 몬스터를 클릭했는지 여부에 대해 bool값으로 리턴 받아서 몬스터를 클릭한 경우에 조건이 일치하도록 했ㄲ고 결과적으로 클릭한 몬스터가 타겟 몬스터와 같은지를 체크한 후 모든 조건이 일치한다면 ismoveble 변수를 false로 만든다
            if (_playerMovement.Pathfinding.target != null) 
            {
                Transform previousTargetTransform = _playerMovement.Pathfinding.target;
                var currentClickValue = _mouseManager.ClickCheck();
                if(_joystickInput.magnitude <= 0 && previousTargetTransform == _playerMovement.Pathfinding.target && currentClickValue)
                {
                    _isMoveAble = false;
                }
            } else
            {
                _mouseManager.ClickCheck();
                _isMoveAble = true;
            }
        }
    }


    public override void Render()
    {
        // 1. setItemIndexs : Init 단계에서 setItemIndexs 네트워크 변수에 값을 할당 한다.
        // 2. AllLoad : SpawnManager 에서 어드레서블 모드 로드 되면 True로 바꿔줌 그니까 리소스가 모두 로드 된 이후에 아래 1회 실행하게 하기 위해 True인 경우에 실행하도록 함
        if (!_isInitialized && setItemIndexs.Length > 0 && StaticManager.Instance.AllLoad == true) 
        {
            _isInitialized = true;
            OnSetitemList();
        }

        float movementSpeed = _kcc.RealSpeed > 0 ? _kcc.RealSpeed / Specs.MovementSpeed : 0;
        _anim.SetFloat("Movement", movementSpeed);
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority) ProcessMovement();
    }
    /// <summary>
    /// 조이스틱으로 움직일 것인지 Astar로 움직일 것인지.
    /// </summary>
    private void ProcessMovement()
    {
        
        if (IsAttack) return; 
        if (_joystick != null)
        {
            _joystickInput = new Vector2(_joystick.Horizontal, _joystick.Vertical);

            // 조이스틱 값이 있을 때 이동 및 회전 처리
            // 조이스틱 값이 있을 때 isMoveAble을 true로 해서 Astar로 움직이지 못 하도록 한다
            if (_joystickInput.magnitude > 0)
            {
                _isMoveAble = true; 
                MoveCharacter(_joystickInput);  // JoyStick Move Logic
            }
            else 
            {
                HandleIdleMovement();           // Astar Move Logic
            }
        }
    }

    /// <summary>
    /// 조이스틱값을 통해 플레이어 이동 및 회전처리 하는 메서드이다.
    /// </summary>
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
        // 조이스틱 값이 없는 경우에만 아래 로직을 탈 수 있다.
        // Astar Move Logic
        if (_playerMovement.Pathfinding.target && !_isMoveAble)
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
            Enemy targetMonster = _playerMovement.Pathfinding.target.GetComponent<Enemy>();
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
    // 죽이면 타겟 해제한느거랑 ㅔ쳑 100 하는거 해야함
    public void AttackRpc(Enemy targetMonster)
    {
        if (targetMonster.NetworkedHealth <= 0)
        {
            _playerMovement.path.Clear();
            _playerMovement.Pathfinding.target = null;
            return;
        }
        else if (targetMonster.NetworkedHealth - 10 <= 0)
        {
            targetMonster.DealDamageRpc(10);
            _playerMovement.path.Clear();
            _playerMovement.Pathfinding.target = null;
            PlayAttackAnimationRpc();
        }
        else
        {
            targetMonster.DealDamageRpc(10);
            PlayAttackAnimationRpc();

            //PushMonster(targetMonster);
        }
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
        StaticManager.DataSetManager.SetCharacterItem(setItemIndexs, itemList, itemParitsList);
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
        bool isAnimationStart = (isAttacking == 1); // TRUE 공격중
        IsAttack = (isAnimationStart) ? true : false;
    }

    public void CalculateStatUI()
    {
        InventoryManager InventoryUI = StaticManager.UI.ContentsInventoryUI;
        List<ItemChart> itemList = BackendGameData.Instance.ItemChartList;

        if (InventoryUI != null)
        {
            int _FinalAtk = default;
            int _FinalHP = default;
            float _FinalAtkSpeed = default;
            float _FinalMoveSpeed = default;
             
            InventoryUI.Power.text = Attack.ToString();
            InventoryUI.Def.text = Def.ToString();
            InventoryUI.Hp.text = Health.ToString();
            
            foreach (var stat in setItemIndexs)
            {                
                foreach(ItemChart item in itemList)
                {
                    if(stat == item.Itemid)
                    {
                        _FinalAtk += item.Damage;
                        _FinalHP += item.Hp;
                        _FinalAtkSpeed += item.AtkSpeed;
                        _FinalMoveSpeed += item.MoveSpeed;
                        break;
                    }
                }
            }
            InventoryUI.LastPower.text = Attack.ToString()+ " + " + _FinalAtk.ToString();
            InventoryUI.LastHp.text = Health.ToString() + " + " + _FinalHP.ToString();
            InventoryUI.LastAtkSpeed.text = "1 + " + _FinalAtkSpeed.ToString();
            InventoryUI.LastMoveSpeed.text = "5 + " + _FinalMoveSpeed.ToString();

            FinalAtk = Attack + _FinalAtk;
            FinalHP = Health + _FinalHP;
            FinalAtkSpeed = 1 + _FinalAtkSpeed;
            FinalMoveSpeed = 5 + _FinalMoveSpeed;
        }
    }
}
