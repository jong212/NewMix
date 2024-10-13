using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AssemblyMap
{
	private static readonly Dictionary<HashSet<IngredientData>, AssemblyMapData> Visuals;

	static AssemblyMap()
	{
		Visuals = new(HashSet<IngredientData>.CreateSetComparer());

		foreach (AssemblyVisual asm in Resources.LoadAll<AssemblyVisual>(""))
		{
			HashSet<IngredientData> ingredientHash = asm.Ingredients.ToHashSet();
			if (Visuals.ContainsKey(ingredientHash))
			{
				Debug.LogWarning($"Duplicate assembly visual definition by '{asm.name}' for the set: {string.Join(", ", asm.Ingredients.Select(i => i.DisplayName))}");
			}
			else
			{
				Visuals.Add(ingredientHash, new AssemblyMapData(asm.Visual, asm.Icon));
				//Debug.Log($"Registered assembly visual: {asm.name} with prefab: {asm.Visual.name} and icon: {asm.Icon.name}", asm);
				// uncomment to debug
			}
		}
	}

	/// <summary>
	/// Ensures that the class constructor has ran.
	/// Call before the data is needed to avoid lag.
	/// </summary>
	public static void Prepare() { }

	public static bool GetVisual(HashSet<IngredientData> ingredients, out GameObject visual)
	{
		if (Visuals.TryGetValue(ingredients, out var data) && data.Visual != null)
		{
			visual = data.Visual;
			return true;
		}
		visual = null;
		return false;
	}

	public static bool GetIcon(HashSet<IngredientData> ingredients, out Sprite icon)
	{
		if (Visuals.TryGetValue(ingredients, out var data))
		{
			icon = data.Icon;
			return true;
		}
		icon = null;
		return false;
	}

	public readonly struct AssemblyMapData
	{
		public readonly GameObject Visual;
		public readonly Sprite Icon;

		public AssemblyMapData(GameObject visual, Sprite icon)
		{
			Visual = visual;
			Icon = icon;
		}
	}
}
