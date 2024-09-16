/// <summary>
/// A WorkSurface gives its item to the interactor
/// </summary>
public class GiveItemToPlayerInteraction : Interaction
{
	public override bool TryInteract(Character interactor)
	{
		if (interactor.HeldItem != null) return false;
		if (interactor.WaitingForAuthority) return false;
		if (!TryGetComponent(out WorkSurface surf)) return false;
		if (surf.ItemOnTop == null) return false;

		LogInteraction();

		interactor.SetHeldItem(surf.ItemOnTop);
		return true;
	}
}
