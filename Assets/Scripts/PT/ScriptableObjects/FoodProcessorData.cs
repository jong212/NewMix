using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cooking/Food Processor")]
public class FoodProcessorData : ScriptableObject
{
	public string DisplayName;
	public FoodProcessor ProcessorPrefab;
}
