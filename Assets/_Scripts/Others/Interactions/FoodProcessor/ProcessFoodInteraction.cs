using Fusion;
using UnityEngine;

public class ProcessFoodInteraction : Interaction
{
	public override bool TryInteract(Character interactor)
	{
		if (interactor && interactor.HeldItem != null) return false;
		if (!TryGetComponent(out FoodProcessor processor)) return false;

		Ingredient ingredient;
		FoodContainer container = null;
		if (TryGetComponent(out WorkSurface surf))
		{
			if (surf.ItemOnTop == null) return false;
			if (!surf.ItemOnTop.TryGetComponent(out ingredient)) return false;
		}
		else if (TryGetComponent(out container))
		{
			if (container.IngredientsCount == 0) return false;
			ingredient = container.ResolveIngredient();
			if (ingredient == null) return false;
		}
		else return false;

		if (!ProcessGraph.GetProcessData(processor.Data, ingredient.Data, out ProcessGraph.ProcessGraphData procData)) return false;

		if (interactor) LogInteraction();

		AuthorityHandler authHandler = surf ? surf.GetComponent<AuthorityHandler>() : container.GetComponent<AuthorityHandler>();

		authHandler.RequestAuthority(() =>
		{
			ingredient.ProcessedAmount += 1f / procData.TicksToComplete;

			if (ingredient.ProcessedAmount >= 1)
			{
				NetworkObject nObj = GetComponent<NetworkObject>();
				Transform itemPoint = surf ? surf.SurfacePoint : container.AssemblyVisualHolder;
				Ingredient resultIngredient = nObj.Runner.Spawn(
					procData.Result.Visual,
					itemPoint.position,
					itemPoint.rotation)
					.GetComponent<Ingredient>();
				resultIngredient.transform.SetParent(itemPoint, true);

				if (container && container.View)
				{
					container.View.UI.Clear();
					container.View.UI.AddIngredient(resultIngredient.Data);
				}

				if (resultIngredient.TryGetComponent(out Rigidbody rb))
				{
					rb.isKinematic = true;
				}

				if (surf) surf.ItemOnTop = resultIngredient.GetComponent<Item>();
				else container.UpdateIngredient(ingredient, resultIngredient);

				nObj.Runner.Despawn(ingredient.Object);
			}
		});

		return true;
	}
}