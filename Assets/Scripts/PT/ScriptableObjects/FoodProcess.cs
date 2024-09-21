using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Cooking/Food Process")]
public class FoodProcess : ScriptableObject
{
	public IngredientData IngredientIn;
	public IngredientData IngredientOut;
	public FoodProcessorData Processor;
	public int TicksToComplete;
}
