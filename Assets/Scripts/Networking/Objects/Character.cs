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
	[SerializeField] private CharacterVisual[] visuals;
	[SerializeField] private Transform uiPoint;

	[Networked, OnChangedRender(nameof(NicknameChanged))] public NetworkString<_16> Nickname { get; set; }
	[Networked, OnChangedRender(nameof(VisualChanged))] public byte Visual { get; set; }
	[Networked] public Item HeldItem { get; set; }
	[Networked] public bool WaitingForAuthority { get; set; }

	public Transform HoldPoint => visuals[Visual].holdPoint;
	public CharacterVisual CharacterVisual => visuals[Visual];

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
            Visual = LocalData.model;
        }
       
        nicknameUI = Instantiate(
            ResourcesManager.instance.worldNicknamePrefab,
            InterfaceManager.instance.worldCanvas.transform);

        NicknameChanged();
        VisualChanged();
    }
    private void Update()
    {
        // 입력 권한이 있는 클라이언트에서만 입력 처리
        if (Object.HasInputAuthority)
        {
			Debug.Log("..");
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Debug.Log("Space bar pressed");
                TryAttack();
            }
        }
    }

    public override void Render()
    {
     
        Animator anim = visuals[Visual].animator;

        // kcc.RealSpeed를 사용하여 Movement 애니메이션 파라미터 설정
        // 캐릭터의 실제 이동 속도에 따라 애니메이션 설정
        anim.SetFloat("Movement", kcc.RealSpeed > 0 ? kcc.RealSpeed / Specs.MovementSpeed : 0);
        anim.SetLayerWeight(1, Mathf.MoveTowards(anim.GetLayerWeight(1), HeldItem ? 1 : 0, 5 * Runner.DeltaTime));

        // 위치 보정: 권한 전송 대기 중일 때 heldItem의 위치 조정
        if (HeldItem != null && HeldItem.Object.StateAuthority != Object.InputAuthority)
        {
            HeldItem.transform.SetPositionAndRotation(HoldPoint.position, HoldPoint.rotation);
        }
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
            Debug.Log($"Joystick Input: {joystickInput}");

            // 조이스틱 입력이 있을 경우 캐릭터 이동 처리
            if (joystickInput.magnitude > 0)
            {
                // 카메라의 회전을 반영한 이동 처리
                Vector3 moveDirection = new Vector3(joystickInput.x, 0, joystickInput.y);
                Debug.Log($"Move Direction: {moveDirection}");

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
                Debug.Log($"Final Move Direction: {finalMoveDirection}");

                // KCC로 캐릭터 이동 처리
                kcc.Move(finalMoveDirection * Specs.MovementSpeed);
                Debug.Log("Character is moving");

                // 캐릭터의 회전 설정 (움직이는 방향을 바라보게)
                if (finalMoveDirection.magnitude > 0)
                {
                    kcc.SetLookRotation(0, Mathf.Atan2(finalMoveDirection.x, finalMoveDirection.z) * Mathf.Rad2Deg);
                    Debug.Log("Character Look Rotation Set");
                }
            }
            else
            {
                // 조이스틱 입력이 없으면 이동 정지
                kcc.Move(Vector3.zero); // 이동을 멈추도록 빈 벡터 전달
                Debug.Log("Character Stopped");
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
                // 몬스터가 맞으면 RPC 호출로 State Authority에게 체력 감소 요청
                targetMonster.DealDamageRpc(10);
            }
        }
    }
    #region Change Detection

    private void NicknameChanged()
	{
		nicknameUI.SetTarget(uiPoint, Nickname.Value);
	}

	private void VisualChanged()
	{
		for (int i = 0; i < visuals.Length; i++)
		{
			visuals[i].gameObject.SetActive(i == Visual);
		}
		//GameManager.instance.ReservedPlayerVisualsChanged();
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
					HeldItem.transform.SetPositionAndRotation(HoldPoint.position, HoldPoint.rotation);
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
		if (HeldItem && HeldItem.TryGetBehaviour(out Throwable throwable))
		{
			Vector3 throwDirection = Quaternion.AngleAxis(-Specs.ThrowArc, HoldPoint.right) * HoldPoint.forward;

			throwable.Throw(throwDirection * Specs.ThrowForce);
			SetHeldItem(null);
		}
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
		if (visuals == null || visuals.Length == 0) return;
		if (Specs == null) return;

		Transform hp = (Runner != null && Runner.IsRunning == true)
			? HoldPoint
			: visuals[0].holdPoint;

		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(hp.position, new Vector3(0.25f, 2f, 0.25f));
		Gizmos.DrawWireSphere(transform.position + transform.forward * Specs.Reach / 2, Specs.Reach / 2);
		Gizmos.DrawWireSphere(transform.position + Vector3.up * 2 + transform.forward * Specs.Reach / 2, Specs.Reach / 2);
	}
}