

public class FabricateItemInteraction : Interaction
{
	public override bool TryInteract(Character interactor)
	{
		if (interactor.HeldItem != null) return false;
		if (interactor.WaitingForAuthority) return false;
		if (!TryGetComponent(out ItemFabricator fab)) return false;
		if (TryGetComponent(out WorkSurface surf) && surf.ItemOnTop) return false;

		LogInteraction();

		Item fabItem = interactor.Runner.Spawn(fab.Item);
		interactor.SetHeldItem(fabItem);

		return true;
	}
}
