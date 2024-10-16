using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class FoodProcessor : NetworkBehaviour
{
	public FoodProcessorData Data;
	public FoodProcessorView View;

	WorkSurface _surf;
	FoodContainer _container;
	Ingredient _ingredient =>
		_surf ? 
			(_surf.ItemOnTop ? _surf.ItemOnTop.GetComponent<Ingredient>() : null)
		:_container ? 
			_container.ResolveIngredient() : null;
	Ingredient _prevIngredient = null;

	public override void Spawned()
	{
		_surf = GetComponent<WorkSurface>();
		_container = GetComponent<FoodContainer>();
	}

	public override void Render()
	{
		if (!Runner.IsForward) return;
		if (!View) return;
		
		if (_ingredient != _prevIngredient)
		{
			View.UI.SetFill(0);
		}

		if (!_ingredient)
		{
			_prevIngredient = _ingredient;
			return;
		}

		if (!ProcessGraph.IsCompatible(Data, _ingredient.Data)) return;

		View.UI.SetFill(_ingredient.ProcessedAmount);

		_prevIngredient = _ingredient;
	}
}
