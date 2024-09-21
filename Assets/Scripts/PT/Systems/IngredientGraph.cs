using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class IngredientGraph
{
    private static readonly Dictionary<IngredientData, HashSet<Recipe>> IngredientRecipes = new();
    private static readonly Dictionary<IngredientData, HashSet<IngredientData>> Compatibilities = new();

    static IngredientGraph()
    {
        foreach (Recipe recipe in Resources.LoadAll<Recipe>(""))
        {
            foreach (IngredientData lhs in recipe.Ingredients)
            {
                if (!IngredientRecipes.ContainsKey(lhs))
                {
                    IngredientRecipes.Add(lhs, new());
                }
                IngredientRecipes[lhs].Add(recipe);

                if (!Compatibilities.ContainsKey(lhs))
                {
                    Compatibilities.Add(lhs, new());
                }
                foreach (IngredientData rhs in recipe.Ingredients)
                {
                    if (lhs != rhs)
                    {
                        Compatibilities[lhs].Add(rhs);
                    }
                }
            }
        }
    }

	/// <summary>
    /// Ensures that the class constructor has ran.
    /// Call before the data is needed to avoid lag.
    /// </summary>
	public static void Prepare() { }

    public static bool IngredientCompatible(IEnumerable<IngredientData> existing, IngredientData toAdd)
    {
		existing = existing.Where(i => i);
		if (!existing.All(e => Compatibilities.ContainsKey(e))) return false;
        return existing.All(e => Compatibilities[e].Contains(toAdd));
    }

    /// <summary>
    /// Returns whether the ingredient is part of any recipe.
    /// </summary>
    public static bool IsRecipeIngredient(IngredientData ingredientData)
    {
        if (IngredientRecipes.TryGetValue(ingredientData, out HashSet<Recipe> set) && set != null && set.Count > 0) return true;
        return false;
    }
}