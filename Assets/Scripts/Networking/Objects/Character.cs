using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;

public class Character : NetworkBehaviour
{
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
		if (Object.HasInputAuthority && Object.HasStateAuthority)
		{
			Nickname = string.IsNullOrWhiteSpace(LocalData.nickname) ? $"Chef{Random.Range(1000, 10000)}" : LocalData.nickname;
			Visual = LocalData.model;
		}

		nicknameUI = Instantiate(
			ResourcesManager.instance.worldNicknamePrefab,
			InterfaceManager.instance.worldCanvas.transform);

		NicknameChanged();
		VisualChanged();
	}

	public override void Render()
	{
		Animator anim = visuals[Visual].animator;
		anim.SetFloat("Movement", kcc.RealSpeed / Specs.MovementSpeed);
		anim.SetLayerWeight(1, Mathf.MoveTowards(anim.GetLayerWeight(1), HeldItem ? 1 : 0, 5 * Runner.DeltaTime));

		// compensate for positional desync while waiting for authority transfer
		if (HeldItem != null && HeldItem.Object.StateAuthority != Object.InputAuthority)
		{
			HeldItem.transform.SetPositionAndRotation(HoldPoint.position, HoldPoint.rotation);
		}
	}

	public override void FixedUpdateNetwork()
	{
		if (GetInput(out PlayerInput input))
		{
			kcc.Move(input.GetMoveVector() * Specs.MovementSpeed);

			if (input.MoveAmount > 0) kcc.SetLookRotation(0, (float)input.MoveDirection);

			if (!WaitingForAuthority && input.GrabPressed(prevInput))
			{
				IEnumerable<Interactable> interactables = GetNearbyInteractables();
				GrabInteractWith(interactables);
			}

			if (input.UsePressed(prevInput))
			{
				IEnumerable<Interactable> interactables = GetNearbyInteractables();
				UseInteractWith(interactables);
			}

			prevInput = input;
		}
		else
		{
			Debug.LogWarning(Nickname + " missed input " + Runner.Tick, gameObject);
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
		GameManager.instance.ReservedPlayerVisualsChanged();
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