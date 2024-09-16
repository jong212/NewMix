using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cooking/Ingredient")]
public class IngredientData : ScriptableObject
{
	[SerializeField] string _displayName;
	public string DisplayName => _displayName;

	[SerializeField] Sprite _icon;
	public Sprite Icon => _icon;

	[SerializeField] GameObject _visual;
	public GameObject Visual => _visual;

	public override string ToString() => DisplayName;
}
