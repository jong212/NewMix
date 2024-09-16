using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cooking/Appliance")]
public class ApplianceData : ScriptableObject
{
	public string DisplayName;
	public FoodProcessorData Processor;
}
