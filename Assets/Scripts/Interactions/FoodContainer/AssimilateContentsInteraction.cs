using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A FoodContainer taking the contents of another FoodContainer.
/// </summary>
public class AssimilateContentsInteraction : Interaction
{
	public override bool TryInteract(Character interactor)
	{
		if (interactor.HeldItem == null) return false;
		if (!interactor.HeldItem.TryGetComponent<FoodContainer>(out var playerContainer)) return false;
		if (!TryGetComponent<FoodContainer>(out var ourContainer)) return false;

		List<Ingredient> ingredients = playerContainer.Ingredients.ToList();
		if (!ourContainer.CanAddIngredients(ingredients)) return false;

		LogInteraction();

		ourContainer.GetComponentTopmost<AuthorityHandler>().RequestAuthority(() =>
		{
			playerContainer.ClearIngredientReferences();
			ourContainer.AddIngredients(ingredients);
		});

		return true;
	}
}
