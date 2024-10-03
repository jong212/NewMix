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
        // �Է� ������ �ִ� Ŭ���̾�Ʈ������ �Է� ó��
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

        // kcc.RealSpeed�� ����Ͽ� Movement �ִϸ��̼� �Ķ���� ����
        // ĳ������ ���� �̵� �ӵ��� ���� �ִϸ��̼� ����
        anim.SetFloat("Movement", kcc.RealSpeed > 0 ? kcc.RealSpeed / Specs.MovementSpeed : 0);
        anim.SetLayerWeight(1, Mathf.MoveTowards(anim.GetLayerWeight(1), HeldItem ? 1 : 0, 5 * Runner.DeltaTime));

        // ��ġ ����: ���� ���� ��� ���� �� heldItem�� ��ġ ����
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

        // ���̽�ƽ �Է� �� �޾ƿ���
        if (joystick != null)
        {
            Vector2 joystickInput = new Vector2(joystick.Horizontal, joystick.Vertical);
            Debug.Log($"Joystick Input: {joystickInput}");

            // ���̽�ƽ �Է��� ���� ��� ĳ���� �̵� ó��
            if (joystickInput.magnitude > 0)
            {
                // ī�޶��� ȸ���� �ݿ��� �̵� ó��
                Vector3 moveDirection = new Vector3(joystickInput.x, 0, joystickInput.y);
                Debug.Log($"Move Direction: {moveDirection}");

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
                Debug.Log($"Final Move Direction: {finalMoveDirection}");

                // KCC�� ĳ���� �̵� ó��
                kcc.Move(finalMoveDirection * Specs.MovementSpeed);
                Debug.Log("Character is moving");

                // ĳ������ ȸ�� ���� (�����̴� ������ �ٶ󺸰�)
                if (finalMoveDirection.magnitude > 0)
                {
                    kcc.SetLookRotation(0, Mathf.Atan2(finalMoveDirection.x, finalMoveDirection.z) * Mathf.Rad2Deg);
                    Debug.Log("Character Look Rotation Set");
                }
            }
            else
            {
                // ���̽�ƽ �Է��� ������ �̵� ����
                kcc.Move(Vector3.zero); // �̵��� ���ߵ��� �� ���� ����
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
        // ����ĳ��Ʈ�� ���� ���(����)�� ã��
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Runner.GetPhysicsScene().Raycast(ray.origin, ray.direction, out var hit))
        {
            // ���Ͱ� �ִ��� Ȯ��
            if (hit.transform.TryGetComponent<Entity>(out var targetMonster))
            {
                // ���Ͱ� ������ RPC ȣ��� State Authority���� ü�� ���� ��û
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