using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodProcessorView : MonoBehaviour
{
	public Transform uiPoint;
	[ReadOnly] public BarUI UI;

	private void Start()
	{
		UI = Instantiate(ResourcesManager.instance.barUIPrefab, InterfaceManager.instance.worldCanvas.transform);
		UI.Init(uiPoint);
		UI.Show(false);
	}

	private void OnDestroy()
	{
		if (UI) Destroy(UI.gameObject);
	}
}
