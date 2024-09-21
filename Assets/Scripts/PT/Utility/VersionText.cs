using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Adjust the version from: Edit > Project Settings > Player > Version 
public class VersionText : MonoBehaviour
{
	public TMP_Text versionText;

	void Awake()
	{
		versionText.text = $"v{Application.version}";
	}
}
