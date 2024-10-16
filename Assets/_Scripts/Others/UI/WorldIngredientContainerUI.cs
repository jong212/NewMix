using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldIngredientContainerUI : WorldUI
{
    public Transform ingredientIconHolder;
	[SerializeField, ReadOnly] private Transform target;

	readonly Dictionary<IngredientData, GameObject> items = new();

	public override void LateUpdate()
	{
		try
		{
			base.LateUpdate();
			transform.position = target.position;
		}
		catch
		{
			Destroy(gameObject);
		}
	}

	public void Init(Transform target)
	{
		this.target = target;
	}

	public void UpdateContents(IEnumerable<Ingredient> source)
	{
		var removed = items.Select(kvp => kvp.Key).Except(source.Select(i => i.Data)).ToArray();
		var added = source.Select(i => i.Data).Except(items.Select(kvp => kvp.Key)).ToArray();

		foreach (IngredientData item in removed)
		{
			Destroy(items[item]);
			items.Remove(item);
		}

		foreach (IngredientData item in added)
		{
			AddIngredient(item);
		}
	}

	public void AddIngredient(IngredientData ingredient)
	{
		WorldIngredientItemUI worldIngredientItemUI = Instantiate(ResourcesManager.instance.worldIngredientItemUIPrefab, ingredientIconHolder);
		worldIngredientItemUI.SetIcon(ingredient.Icon);
		items.Add(ingredient, worldIngredientItemUI.gameObject);
	}

	public void Clear()
	{
		ingredientIconHolder.DestroyChildren();
		items.Clear();
	}
}
