/// <summary>
/// Dispose of the contents of a held container.
/// </summary>
public class VoidContentsInteraction : Interaction
{
	public override bool TryInteract(Character interactor)
	{
		if (interactor.HeldItem == null) return false;
		if (!interactor.HeldItem.TryGetComponent<FoodContainer>(out var playerContainer)) return false;

		LogInteraction();

		playerContainer.AssemblyVisualHolder.DestroyChildren();
		if (playerContainer.View) playerContainer.View.UI.Clear();

		foreach (var ing in playerContainer.Ingredients)
		{
			ing.Runner.Despawn(ing.Object);
		}
		playerContainer.ClearIngredientReferences();

		return true;
	}
}
