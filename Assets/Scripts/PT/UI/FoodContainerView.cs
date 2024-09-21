using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodContainerView : MonoBehaviour
{
	public Transform uiPoint;
	[field: SerializeField, ReadOnly] public WorldIngredientContainerUI UI { get; private set; }

	private void Start()
	{
		UI = Instantiate(ResourcesManager.instance.worldIngredientContainerUIPrefab, InterfaceManager.instance.worldCanvas.transform);
		UI.Init(uiPoint);
	}
}
