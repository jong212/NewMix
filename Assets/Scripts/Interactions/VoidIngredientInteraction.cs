/// <summary>
/// Dispose of a held ingredient.
/// </summary>
public class VoidIngredientInteraction : Interaction
{
	public override bool TryInteract(Character interactor)
	{
		if (interactor.HeldItem == null) return false;
		if (interactor.WaitingForAuthority) return false;
		if (!interactor.HeldItem.TryGetComponent<Ingredient>(out _)) return false;

		LogInteraction();

		interactor.HeldItem.Runner.Despawn(interactor.HeldItem.Object);
		interactor.SetHeldItem(null);

		return true;
	}
}
