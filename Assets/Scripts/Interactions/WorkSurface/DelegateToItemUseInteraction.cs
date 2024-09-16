/// <summary>
/// Delegate interaction to the item on top of a workstation
/// </summary>
public class DelegateToItemUseInteraction : Interaction
{
	public override bool TryInteract(Character interactor)
	{
		if (!TryGetComponent(out WorkSurface surf)) return false;
		if (surf.ItemOnTop == null) return false;
		if (!surf.ItemOnTop.TryGetComponent(out Interactable interactable)) return false;

		LogInteraction();

		return interactable.UseInteract(interactor);
	}
}
