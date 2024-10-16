using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Cooking/Recipe")]
public class Recipe : ScriptableObject
{
    public IngredientData[] RequiredIngredients;
	public IngredientData[] OptionalIngredients;

	public IEnumerable<IngredientData> Ingredients => RequiredIngredients.Concat(OptionalIngredients);
}
