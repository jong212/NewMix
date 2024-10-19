using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine.InputSystem;

public class Character : NetworkBehaviour
{
    private VariableJoystick joystick; // Joystick ����

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
        // KCCCollider ������Ʈ�� �ڽ� ������Ʈ���� ã���ϴ�.
        Transform kccColliderTransform = transform.Find("KCCCollider");
        if (kccColliderTransform != null)
        {
            CapsuleCollider kccCollider = kccColliderTransform.GetComponent<CapsuleCollider>();

            if (kccCollider != null)
            {
                // �ʿ��� Collider ���� ����
                // ��: isTrigger�� �����Ͽ� ������ �浹�� �����ϵ��� ����
                kccCollider.isTrigger = false;

                // Collider�� ũ�� �� �ٸ� �Ӽ� ����
                //kccCollider.radius = 0.97f; // ���ϴ� ������ ����
                //kccCollider.height = 0.37f;    // ���ϴ� ������ ����

                // �߰��� �ʿ��� ������ �ִٸ� ���⿡ �߰�
            }
            else
            {
                Debug.LogWarning("KCCCollider�� CapsuleCollider�� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning("KCCCollider ������Ʈ�� ã�� �� �����ϴ�.");
        }
    }
    private void Update()
    {
        // �Է� ������ �ִ� Ŭ���̾�Ʈ������ �Է� ó��
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

        // kcc.RealSpeed�� ����Ͽ� Movement �ִϸ��̼� �Ķ���� ����
        // ĳ������ ���� �̵� �ӵ��� ���� �ִϸ��̼� ����
        anim.SetFloat("Movement", kcc.RealSpeed > 0 ? kcc.RealSpeed / Specs.MovementSpeed : 0);

        // ��ġ ����: ���� ���� ��� ���� �� heldItem�� ��ġ ����
     
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority)
        {
            return;
        }

        // ���̽�ƽ �Է� �� �޾ƿ���
        if (joystick != null)
        {
            Vector2 joystickInput = new Vector2(joystick.Horizontal, joystick.Vertical);
            //Debug.Log($"Joystick Input: {joystickInput}");

            // ���̽�ƽ �Է��� ���� ��� ĳ���� �̵� ó��
            if (joystickInput.magnitude > 0)
            {
                // ī�޶��� ȸ���� �ݿ��� �̵� ó��
                Vector3 moveDirection = new Vector3(joystickInput.x, 0, joystickInput.y);
               // Debug.Log($"Move Direction: {moveDirection}");

                // ī�޶��� ȸ�� ����� �����ͼ� �̵� ������ ��ȯ
                Vector3 cameraForward = Camera.main.transform.forward;
                Vector3 cameraRight = Camera.main.transform.right;

                // ī�޶��� ���̸� �����ϰ� ���󿡼��� �̵�
                cameraForward.y = 0;
                cameraRight.y = 0;
                cameraForward.Normalize();
                cameraRight.Normalize();

                // ī�޶� �������� ���̽�ƽ ������ ��ȯ
                Vector3 finalMoveDirection = cameraForward * moveDirection.z + cameraRight * moveDirection.x;
                //Debug.Log($"Final Move Direction: {finalMoveDirection}");

                // KCC�� ĳ���� �̵� ó��
                kcc.Move(finalMoveDirection * Specs.MovementSpeed);
                //Debug.Log("Character is moving");

                // ĳ������ ȸ�� ���� (�����̴� ������ �ٶ󺸰�)
                if (finalMoveDirection.magnitude > 0)
                {
                    kcc.SetLookRotation(0, Mathf.Atan2(finalMoveDirection.x, finalMoveDirection.z) * Mathf.Rad2Deg);
                   // Debug.Log("Character Look Rotation Set");
                }
            }
            else
            {
                // ���̽�ƽ �Է��� ������ �̵� ����
                kcc.Move(Vector3.zero); // �̵��� ���ߵ��� �� ���� ����
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
        // ����ĳ��Ʈ�� ���� ���(����)�� ã��
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Runner.GetPhysicsScene().Raycast(ray.origin, ray.direction, out var hit))
        {
            // ���Ͱ� �ִ��� Ȯ��
            if (hit.transform.TryGetComponent<Entity>(out var targetMonster))
            {
                targetMonster.DealDamageRpc(10);
                // ���Ͱ� ������ �б� ���� ����
                PushMonster(targetMonster);
            }
        }
    }

    void PushMonster(Entity monster)
    {
        // ������ Rigidbody�� ������
        Rigidbody monsterRb = monster.GetComponent<Rigidbody>();

        if (monsterRb != null)
        {
            // �÷��̾�� ������ ��ġ ���̸� ������� ������ ����
            Vector3 pushDirection = (monster.transform.position - transform.position).normalized;

            // �ڷ� �̴� ���� ���� (ForceMode.Impulse�� ��� �� ����)
            float pushForce = 50f; // ���� ũ�⸦ ����
            monsterRb.isKinematic = false;
            monsterRb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
        }
        else
        {
            // Rigidbody�� ������ ��ġ�� ���� ����
            Vector3 pushDirection = (monster.transform.position - transform.position).normalized;
            monster.transform.position += pushDirection * 0.5f; // �и��� ������ ����
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
