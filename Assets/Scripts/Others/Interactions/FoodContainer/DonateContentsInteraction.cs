using System.Linq;

/// <summary>
/// A FoodContainer giving its contents to another FoodContainer.
/// </summary>
public class DonateContentsInteraction : Interaction
{
    public override bool TryInteract(Character interactor)
    {
        if (interactor.HeldItem == null) return false;
        if (!interactor.HeldItem.TryGetComponent<FoodContainer>(out var playerContainer)) return false;
        if (!TryGetComponent<FoodContainer>(out var ourContainer)) return false;

        if (!playerContainer.CanAddIngredients(ourContainer.Ingredients.Where(i => i).ToList())) return false;

		LogInteraction();

        ourContainer.GetComponentTopmost<AuthorityHandler>().RequestAuthority(() =>
        {
            playerContainer.AddIngredients(ourContainer.Ingredients);
            ourContainer.ClearIngredientReferences();
        });
		
        return true;
    }
}