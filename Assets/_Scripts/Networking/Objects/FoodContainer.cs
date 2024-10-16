using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;

public class FoodContainer : NetworkBehaviour
{
	public int Capacity;
	[field: SerializeField] public bool UseAssemblyVisuals { get; private set; } = true;
	[field: SerializeField] public bool RecipeIngredientsOnly { get; private set; } = false;
	[Networked, Capacity(5), OnChangedRender(nameof(IngredientsChanged))] private NetworkArray<NetworkBehaviourId> IngredientsArray => default;
	[Networked] public byte IngredientsCount { get; set; }
	public IEnumerable<Ingredient> Ingredients => IngredientsArray.Select(i => Runner.TryFindBehaviour(i, out Ingredient b) ? b : null).Where(i => i != null);

	[field: SerializeField] public Transform SurfacePoint { get; private set; }
	[field: SerializeField] public Transform AssemblyVisualHolder { get; private set; }
	public FoodContainerView View;

	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		if (hasState)
		{
			foreach (var ingr in Ingredients)
			{
				runner.Despawn(ingr.Object);
			}
		}
	}

	private void IngredientsChanged()
	{
		if (View) View.UI.UpdateContents(Ingredients);

		AssemblyVisualHolder.DestroyChildren();

		if (UseAssemblyVisuals && AssemblyMap.GetVisual(Ingredients.Select(i => i.Data).ToHashSet(), out GameObject visual))
		{
			foreach (var ingredient in Ingredients)
			{
				ingredient.Visual.SetActive(false);
			}

			GameObject go = Instantiate(visual, AssemblyVisualHolder.position, AssemblyVisualHolder.rotation, AssemblyVisualHolder);
		}
		else
		{
			foreach (var ingredient in Ingredients)
			{
				ingredient.Visual.SetActive(true);
			}
		}
	}

	private void IngredientsChangedAuth()
	{
		foreach (var ingredient in Ingredients)
		{
			if (ingredient)
			{
				ingredient.transform.SetPositionAndRotation(SurfacePoint.position, SurfacePoint.rotation);
				ingredient.transform.SetParent(transform, true);
			}
		}
	}

	public bool CanAddIngredient(Ingredient ingredient)
	{
		if (IngredientsCount == Capacity) return false;
		if (RecipeIngredientsOnly && !IngredientGraph.IsRecipeIngredient(ingredient.Data)) return false;
		if (!IngredientGraph.IngredientCompatible(Ingredients.Select(i => i.Data), ingredient.Data)) return false;

		return true;
	}

	public void AddIngredient(Ingredient ingredient)
	{
		IngredientsArray.Set(IngredientsCount, ingredient.Id);
		IngredientsCount++;
		IngredientsChangedAuth();
	}

	public bool CanAddIngredients(List<Ingredient> toAdd)
	{
		if (toAdd.Count == 0) return false;
		if (IngredientsCount + toAdd.Count > Capacity) return false;
		if (RecipeIngredientsOnly && toAdd.Any(i => !IngredientGraph.IsRecipeIngredient(i.Data))) return false;
		if (toAdd.Any(a => !IngredientGraph.IngredientCompatible(Ingredients.Select(i => i.Data), a.Data))) return false;

		return true;
	}

	public void AddIngredients(IEnumerable<Ingredient> ingredients)
	{
		foreach (var ingredient in ingredients)
		{
			IngredientsArray.Set(IngredientsCount, ingredient.Id);
			IngredientsCount++;
		}
		IngredientsChangedAuth();
	}

	public bool UpdateIngredient(Ingredient from, Ingredient to)
	{
		if (IngredientsArray.Contains(from))
		{
			Rpc_UpdateIngredient(from, to);
			return true;
		}

		return false;
	}

	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	void Rpc_UpdateIngredient(Ingredient from, Ingredient to)
	{
		for (int i = 0; i < IngredientsArray.Length; i++)
		{
			if (IngredientsArray[i] == from.Id)
			{
				IngredientsArray.Set(i, to.Id);
				IngredientsChangedAuth();
				return;
			}
		}
	}

	public void ClearIngredientReferences()
	{
		for (int i = 0; i < IngredientsArray.Length; i++)
		{
			IngredientsArray.Set(i, default);
		}
		IngredientsCount = 0;
		IngredientsChangedAuth();
	}

	public Ingredient ResolveIngredient(int index = 0)
	{
		return Runner.TryFindBehaviour(IngredientsArray[index], out Ingredient ingredient) ? ingredient : null;
	}
}