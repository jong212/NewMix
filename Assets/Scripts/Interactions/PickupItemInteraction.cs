/// <summary>
/// The "Default Interaction" for a lot of objects. The player picks us up.
/// </summary>
public class PickupItemInteraction : Interaction
{
	public override bool TryInteract(Character interactor)
	{
		if (interactor.HeldItem != null) return false;
		if (interactor.WaitingForAuthority) return false;
		if (!TryGetComponent(out Item item)) return false;

		LogInteraction();

		interactor.SetHeldItem(item);

		return true;
	}
}