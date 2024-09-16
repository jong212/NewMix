using System.Collections.Generic;
using UnityEngine;

public class OrderMenu : MonoBehaviour
{
	[SerializeField] Recipe[] initialRecipes;

	public static readonly List<Recipe> Recipes = new();

	private void Start()
	{
		Recipes.Clear();
		foreach (var r in initialRecipes) AddRecipe(r);
	}

	public static void AddRecipe(Recipe recipe)
	{
		if (Recipes.Contains(recipe)) return;
		Recipes.Add(recipe);
	}

	public static IngredientData[] GetOrder()
	{
		List<IngredientData> orderIngr = new();
		Recipe recipe = Recipes[Random.Range(0, Recipes.Count)];
		orderIngr.AddRange(recipe.RequiredIngredients);
		
		float optsRecip = 1f / recipe.OptionalIngredients.Length;
		for (int i = 0; i < recipe.OptionalIngredients.Length; i++)
		{
			if (Random.value >= optsRecip)
			{
				orderIngr.Add(recipe.OptionalIngredients[i]);
			}
		}
		return orderIngr.ToArray();
	} 
}
