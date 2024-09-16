using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cooking/Assembly Visual")]
public class AssemblyVisual : ScriptableObject
{
	public IngredientData[] Ingredients;
	public GameObject Visual;
	public Sprite Icon;
}
