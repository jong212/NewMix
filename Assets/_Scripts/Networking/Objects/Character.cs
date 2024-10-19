using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine.InputSystem;

public class Character : NetworkBehaviour
{
    private VariableJoystick joystick; // Joystick 참조

    [field: SerializeField] public CharacterSpecs Specs { get; private set; }
    [SerializeField] private SimpleKCC kcc;
    [SerializeField] private Transform uiPoint;
    [SerializeField] private Animator anim;

    [Networked, OnChangedRender(nameof(NicknameChanged))] public NetworkString<_16> Nickname { get; set; }
    [Networked] public Item HeldItem { get; set; }
    [Networked] public bool WaitingForAuthority { get; set; }


    private PlayerInput prevInput;
    private WorldNickname nicknameUI = null;

    public override void Spawned()
    {
        joystick = FindObjectOfType<VariableJoystick>();

        if (Object.HasInputAuthority && Object.HasStateAuthority)
        {
            IsometricCameraFollow cameraFollow = FindObjectOfType<IsometricCameraFollow>();
            cameraFollow.target = this.transform;

            Nickname = string.IsNullOrWhiteSpace(LocalData.nickname) ? $"Chef{Random.Range(1000, 10000)}" : LocalData.nickname;

        }

        nicknameUI = Instantiate(
            ResourcesManager.instance.worldNicknamePrefab,
            InterfaceManager.instance.worldCanvas.transform);

        NicknameChanged();
        ModifyKCCCollider();
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
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Debug.Log("Space bar pressed");
                TryAttack();
            }
        }
    }

    public override void Render()
    {

        // kcc.RealSpeed를 사용하여 Movement 애니메이션 파라미터 설정
        // 캐릭터의 실제 이동 속도에 따라 애니메이션 설정
        anim.SetFloat("Movement", kcc.RealSpeed > 0 ? kcc.RealSpeed / Specs.MovementSpeed : 0);

        // 위치 보정: 권한 전송 대기 중일 때 heldItem의 위치 조정
     
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority)
        {
            return;
        }

        // 조이스틱 입력 값 받아오기
        if (joystick != null)
        {
            Vector2 joystickInput = new Vector2(joystick.Horizontal, joystick.Vertical);
            //Debug.Log($"Joystick Input: {joystickInput}");

            // 조이스틱 입력이 있을 경우 캐릭터 이동 처리
            if (joystickInput.magnitude > 0)
            {
                // 카메라의 회전을 반영한 이동 처리
                Vector3 moveDirection = new Vector3(joystickInput.x, 0, joystickInput.y);
               // Debug.Log($"Move Direction: {moveDirection}");

                // 카메라의 회전 행렬을 가져와서 이동 방향을 변환
                Vector3 cameraForward = Camera.main.transform.forward;
                Vector3 cameraRight = Camera.main.transform.right;

                // 카메라의 높이를 무시하고 평면상에서만 이동
                cameraForward.y = 0;
                cameraRight.y = 0;
                cameraForward.Normalize();
                cameraRight.Normalize();

                // 카메라 기준으로 조이스틱 방향을 변환
                Vector3 finalMoveDirection = cameraForward * moveDirection.z + cameraRight * moveDirection.x;
                //Debug.Log($"Final Move Direction: {finalMoveDirection}");

                // KCC로 캐릭터 이동 처리
                kcc.Move(finalMoveDirection * Specs.MovementSpeed);
                //Debug.Log("Character is moving");

                // 캐릭터의 회전 설정 (움직이는 방향을 바라보게)
                if (finalMoveDirection.magnitude > 0)
                {
                    kcc.SetLookRotation(0, Mathf.Atan2(finalMoveDirection.x, finalMoveDirection.z) * Mathf.Rad2Deg);
                   // Debug.Log("Character Look Rotation Set");
                }
            }
            else
            {
                // 조이스틱 입력이 없으면 이동 정지
                kcc.Move(Vector3.zero); // 이동을 멈추도록 빈 벡터 전달
              // Debug.Log("Character Stopped");
            }
        }
        else
        {
            Debug.LogWarning("Joystick not found in the scene.", gameObject);
        }
    }

    void TryAttack()
    {
        // 레이캐스트로 공격 대상(몬스터)을 찾음
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Runner.GetPhysicsScene().Raycast(ray.origin, ray.direction, out var hit))
        {
            // 몬스터가 있는지 확인
            if (hit.transform.TryGetComponent<Entity>(out var targetMonster))
            {
                targetMonster.DealDamageRpc(10);
                // 몬스터가 맞으면 밀기 로직 실행
                PushMonster(targetMonster);
            }
        }
    }

    void PushMonster(Entity monster)
    {
        // 몬스터의 Rigidbody를 가져옴
        Rigidbody monsterRb = monster.GetComponent<Rigidbody>();

        if (monsterRb != null)
        {
            // 플레이어와 몬스터의 위치 차이를 기반으로 방향을 설정
            Vector3 pushDirection = (monster.transform.position - transform.position).normalized;

            // 뒤로 미는 힘을 적용 (ForceMode.Impulse로 즉시 힘 적용)
            float pushForce = 50f; // 힘의 크기를 조절
            monsterRb.isKinematic = false;
            monsterRb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
        }
        else
        {
            // Rigidbody가 없으면 위치를 직접 조정
            Vector3 pushDirection = (monster.transform.position - transform.position).normalized;
            monster.transform.position += pushDirection * 0.5f; // 밀리는 정도를 조절
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

    private void GrabInteractWith(IEnumerable<Interactable> interactables)
    {
        foreach (var interactable in interactables)
        {
            if (interactable.GrabInteract(this)) return;
        }

        // Drop held object if it is physical
      
    }

    private void UseInteractWith(IEnumerable<Interactable> interactables)
    {
        if (interactables.Count() != 0)
        {
            foreach (var interactable in interactables)
            {
                if (interactable.UseInteract(this)) return;
            }
        }
    }

    private IEnumerable<Interactable> GetNearbyInteractables()
    {
        Vector3 p0 = transform.position + transform.forward * Specs.Reach / 2;
        return Physics.OverlapCapsule(p0, p0 + Vector3.up * 2, Specs.Reach / 2)
            .Select(c => c.GetComponentInParent<Interactable>())
            .Where(a => a != null)
            .OrderBy(h => Vector3.Distance(p0, h.transform.position));
    }

    private void OnDrawGizmos()
    {

    }
}
