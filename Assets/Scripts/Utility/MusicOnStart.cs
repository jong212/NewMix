using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicOnStart : MonoBehaviour
{
	public string music;

	private void Start()
	{
		AudioManager.PlayMusic(music);
	}
}
