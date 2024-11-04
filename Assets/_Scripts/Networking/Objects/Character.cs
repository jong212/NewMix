using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class Character : NetworkBehaviour
{
    private VariableJoystick joystick; // Joystick 참조

    [field: SerializeField] public CharacterSpecs Specs { get; private set; }
    [SerializeField] private SimpleKCC kcc;
    [SerializeField] private Transform uiPoint;
    [SerializeField] private Animator anim;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private MouseManager mouseManager;
    [Networked, OnChangedRender(nameof(NicknameChanged))] public NetworkString<_16> Nickname { get; set; }
    [Networked] public int _level { get; set; }
    [Networked] public int _atk { get; set; }
    [Networked] public int _hp { get; set; }
    [Networked] public int _miss { get; set; }
    [Networked] public bool WaitingForAuthority { get; set; }
    [Networked] public Item HeldItem { get; set; }

    private PlayerInput prevInput;
    private WorldNickname nicknameUI = null;
    private Vector2 joystickInput;
    private bool isMoveAble { get; set; }
    
    public override void Spawned()
    {
        joystick = FindObjectOfType<VariableJoystick>();

        if (Object.HasInputAuthority && Object.HasStateAuthority)
        {
            StaticManager.UI.CommonOpen(UIType.BtnAttack, StaticManager.UI.MainUI.Layout_BottomRight, true, ChangeMoveProperty);
            IsometricCameraFollow cameraFollow = FindObjectOfType<IsometricCameraFollow>();
            cameraFollow.target = this.transform;

            Nickname = BackendGameData.Instance.NickName;
            InitPlayerInfo();
        }

        nicknameUI = Instantiate(
            StaticManager.UI.WorldNickNameUI,
            StaticManager.Instance.WorldCanvas.transform);
        NicknameChanged();

        ModifyKCCCollider();
    }
    private void InitPlayerInfo()
    {
        UserData test = BackendGameData.Instance.userData;                      // 플레이어 데이터 캐싱된 거 가져옴
        _level = test.level;
        _atk = test.atk;
        _hp = test.hp;
        _miss = test.miss;
        Debug.Log($"[5 PlayerSpawner : 플레이어 스폰 완료 => 데이터 세팅 완료{"레벨" + test.level}{"힘" + test.atk}{"체력" + test.hp}  ]");
    }
    private void ModifyKCCCollider()
    {
        // KCCCollider 오브젝트를 자식 오브젝트에서 찾습니다.
        Transform kccColliderTransform = transform.Find("KCCCollider");
        if (kccColliderTransform != null)
        {
            CapsuleCollider kccCollider = kccColliderTransform.GetComponent<CapsuleCollider>();

            if (kccCollider != null)
            {
                // 필요한 Collider 설정 변경
                // 예: isTrigger를 해제하여 물리적 충돌이 가능하도록 설정
                kccCollider.isTrigger = false;

                // Collider의 크기 등 다른 속성 변경
                //kccCollider.radius = 0.97f; // 원하는 값으로 설정
                //kccCollider.height = 0.37f;    // 원하는 값으로 설정
                // 추가로 필요한 설정이 있다면 여기에 추가
            }
            else
            {
                Debug.LogWarning("KCCCollider에 CapsuleCollider가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("KCCCollider 오브젝트를 찾을 수 없습니다.");
        }
    }
    private void Update()
    {
        
        // 입력 권한이 있는 클라이언트에서만 입력 처리
        if (Object.HasInputAuthority)
        {
            if (Input.GetMouseButtonDown(0))
            {
                mouseManager.ClickCheck();
                if(playerMovement.Pathfinding.target && !(joystickInput.magnitude > 0))
                {
                    isMoveAble = false;
                }
            }

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Debug.Log("Space bar pressed");
                TryAttack();
            }
        }
    }
    // kcc.RealSpeed를 사용하여 Movement 애니메이션 파라미터 설정
    public override void Render()
    {
        anim.SetFloat("Movement", kcc.RealSpeed > 0 ? kcc.RealSpeed / Specs.MovementSpeed : 0); // 캐릭터의 실제 이동 속도에 따라 애니메이션 설정
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority)
        {
            return;
        }
        
        // 조이스틱 이동 잠시 중지
        if (joystick != null)
        {
             joystickInput = new Vector2(joystick.Horizontal, joystick.Vertical);
            if (joystickInput.magnitude > 0)                                                // 조이스틱 입력이 있을 경우 캐릭터 이동 처리
            {
                isMoveAble = true;
                Vector3 moveDirection = new Vector3(joystickInput.x, 0, joystickInput.y);   // 카메라의 회전을 반영한 이동 처리
                Vector3 cameraForward = Camera.main.transform.forward;                      // 카메라의 회전 행렬을 가져와서 이동 방향을 변환
                Vector3 cameraRight = Camera.main.transform.right;                          
                                                                                            
                cameraForward.y = 0;                                                        // 카메라의 높이를 무시하고 평면상에서만 이동
                cameraRight.y = 0;
                cameraForward.Normalize();
                cameraRight.Normalize();
                Vector3 finalMoveDirection = cameraForward * moveDirection.z + cameraRight * moveDirection.x; // 카메라 기준으로 조이스틱 방향을 변환
                
                kcc.Move(finalMoveDirection * Specs.MovementSpeed);                         // KCC로 캐릭터 이동 처리

                if (finalMoveDirection.magnitude > 0)                                       // 캐릭터의 회전 설정 (움직이는 방향을 바라보게)
                {
                    kcc.SetLookRotation(0, Mathf.Atan2(finalMoveDirection.x, finalMoveDirection.z) * Mathf.Rad2Deg);
                }
            }  
            else
            {

                if (playerMovement.Pathfinding.target && !isMoveAble)
                {
                    playerMovement.Movement();
                    return;
                }                
                kcc.Move(Vector3.zero); // 조이스틱 입력이 없으면 이동 정지
            }
        }
    }
    // 몬스터 공격 메서드
    void TryAttack()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);            // 레이캐스트로 공격 대상(몬스터)을 찾음
        if (Runner.GetPhysicsScene().Raycast(ray.origin, ray.direction, out var hit))
        {
            if (hit.transform.TryGetComponent<Entity>(out var targetMonster))   // 몬스터가 있는지 확인
            {
                targetMonster.DealDamageRpc(10);                                // 몬스터가 맞으면 밀기 로직 실행
                PushMonster(targetMonster);
            }
        }
    }
    // 플레이어가 몬스터 공격 시 몬스터 밀기
    void PushMonster(Entity monster)
    {
        Rigidbody monsterRb = monster.GetComponent<Rigidbody>();                                    // 몬스터의 Rigidbody를 가져옴

        if (monsterRb != null)
        {
            Vector3 pushDirection = (monster.transform.position - transform.position).normalized;   // 플레이어와 몬스터의 위치 차이를 기반으로 방향을 설정
            float pushForce = 50f;                                                                  // 힘의 크기를 조절
            monsterRb.isKinematic = false;
            monsterRb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
        }
        else
        {
            Vector3 pushDirection = (monster.transform.position - transform.position).normalized;  // Rigidbody가 없으면 위치를 직접 조정
            monster.transform.position += pushDirection * 0.5f;                                    // 밀리는 정도를 조절
        }
    }
    #region Change Detection
    private void NicknameChanged()
    {
     nicknameUI.SetTarget(uiPoint, Nickname.Value);
   }
    #endregion
    public void SetHeldItem(Item item)
    {
        if (item == null)
        {
            HeldItem = null;
            kcc.RefreshChildColliders();
        }
        else
        {
            AuthorityHandler authHandler = item.GetComponentTopmost<AuthorityHandler>();
            if (authHandler.TryGetComponent(out Character _)) return;

            WaitingForAuthority = true;

            authHandler.RequestAuthority(
                onAuthorized: () =>
                {
                    if (item.TryGetComponent(out Rigidbody rb))
                    {
                        rb.isKinematic = true;
                    }

                    if (item.TryGetComponent(out ColliderGroup cg))
                    {
                        cg.CollidersEnabled = false;
                    }

                    if (authHandler.TryGetComponent(out WorkSurface surf)) surf.ItemOnTop = null;

                    WaitingForAuthority = false;
                    HeldItem = item;
                    HeldItem.transform.SetParent(Object.transform);
                    kcc.RefreshChildColliders();
                },
                onUnauthorized: () => WaitingForAuthority = false
            );
        }
    }
    private void ChangeMoveProperty()
    {
        isMoveAble = false;
    }
}
