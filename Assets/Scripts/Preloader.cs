using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Preloader : MonoBehaviour
{
	public float lengthOfTime = 3;

	private void Start()
	{
		StartCoroutine(Preload(lengthOfTime));
	}

	public IEnumerator Preload(float length)
	{
		yield return new WaitForSeconds(length);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
