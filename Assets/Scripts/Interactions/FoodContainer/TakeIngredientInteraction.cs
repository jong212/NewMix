/// <summary>
/// A FoodContainer taking an Ingredient from the Player.
/// </summary>
public class TakeIngredientInteraction : Interaction
{
    public override bool TryInteract(Character interactor)
    {
        if (interactor.HeldItem == null) return false;
        if (interactor.WaitingForAuthority) return false;
        if (!interactor.HeldItem.TryGetComponent(out Ingredient ingredient)) return false;
        if (!TryGetComponent(out FoodContainer container)) return false;

        if (!container.CanAddIngredient(ingredient)) return false;

        LogInteraction();

        interactor.WaitingForAuthority = true;
        container.GetComponentTopmost<AuthorityHandler>().RequestAuthority(
            onAuthorized: () =>
            {
                interactor.WaitingForAuthority = false;
                ingredient.transform.SetPositionAndRotation(container.SurfacePoint.position, container.SurfacePoint.rotation);
                interactor.SetHeldItem(null);
                container.AddIngredient(ingredient);
            },
            onUnauthorized: () => interactor.WaitingForAuthority = false
        );

        return true;
    }
}