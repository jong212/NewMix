using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class QuitButton : MonoBehaviour
{
	Button button;

	private void Awake()
	{
#if UNITY_WEBGL
		// We don't need a Quit button in WebGL
		gameObject.SetActive(false);
#else
		button = GetComponent<Button>();
		button.onClick.AddListener(Quit);
#endif
	}

	public void Quit()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
		Application.Quit();
	}
}