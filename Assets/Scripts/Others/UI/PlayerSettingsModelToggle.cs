using UnityEngine;
using UnityEngine.UI;

public class PlayerSettingsModelToggle : MonoBehaviour
{
	[SerializeField] private byte modelIndex;
	public CanvasGroup canvasGroup;
	public GameObject unavailableIconObj, tickIconObj;

	private void OnEnable()
	{
		GameManager.OnReservedPlayerVisualsChanged += ReservedVisualsChanged;
	}

	private void OnDisable()
	{
		GameManager.OnReservedPlayerVisualsChanged -= ReservedVisualsChanged;
	}

	void ReservedVisualsChanged(byte reservedMask)
	{
		SetAvailable((reservedMask & (byte)(1 << modelIndex)) == 0);
	}

	public void OnToggled(bool isOn)
	{
		if (isOn) LocalData.model = modelIndex;
	}

	public void SetAvailable(bool available)
	{
		unavailableIconObj.SetActive(!available);
		tickIconObj.SetActive(available);
		canvasGroup.alpha = available ? 1 : 0.5f;
		canvasGroup.interactable = available;
    }
}
