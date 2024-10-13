using UnityEngine;

/// <summary>
/// A WorkSurface receives an item from the interactor
/// </summary>
public class PlaceItemFromPlayerInteraction : Interaction
{
	public override bool TryInteract(Character interactor)
	{
		if (interactor.HeldItem == null) return false;
		if (interactor.WaitingForAuthority) return false;
		if (!TryGetComponent(out WorkSurface surf)) return false;
		if (surf.ItemOnTop != null) return false;

		LogInteraction();

		interactor.WaitingForAuthority = true;
		surf.GetComponent<AuthorityHandler>().RequestAuthority(
			onAuthorized: () =>
			{
				interactor.WaitingForAuthority = false;
				interactor.HeldItem.transform.SetPositionAndRotation(surf.SurfacePoint.position, surf.SurfacePoint.rotation);
				interactor.HeldItem.transform.SetParent(surf.Object.transform, true);
				surf.ItemOnTop = interactor.HeldItem;
				interactor.SetHeldItem(null);
			},
			onUnauthorized: () => interactor.WaitingForAuthority = false
		);

		return true;
	}
}