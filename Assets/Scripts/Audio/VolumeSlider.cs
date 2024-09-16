using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
	private float lastVal;

	private void OnEnable()
	{
		if (TryGetComponent(out Slider slider))
		{
			lastVal = slider.value = AudioManager.GetPref(AudioManager.mainVolumeParam);
			slider.onValueChanged.AddListener((val) =>
			{
				AudioManager.SetVolumeMaster(val);
				if (Mathf.Round(val * 10) != Mathf.Round(lastVal * 10))
				{
					AudioManager.Play("hoverUI", AudioManager.MixerTarget.SFX);
					lastVal = val;
				}
			});
		}
	}
}