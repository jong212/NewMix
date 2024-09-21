using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helpers.Math;

public class ShakeBehaviour : MonoBehaviour
{
	public float shakeFactor = 0.01f;
	public float stoppingThreshold = 0.01f;
    [SerializeField, ReadOnly] float magnitude = 0f;
	[SerializeField, ReadOnly] float lambda = 0f;


	public void TriggerShake(float magnitude, float lambda)
    {
		if (magnitude >= this.magnitude)
		{
			this.magnitude = magnitude;
			this.lambda = lambda;
		}
    }

	private void Update()
	{
		if (magnitude > stoppingThreshold)
		{
			magnitude = MathUtil.Damp(magnitude, 0, lambda);
			transform.localPosition = Random.insideUnitSphere * magnitude * shakeFactor;

			if (magnitude <= stoppingThreshold)
			{
				magnitude = 0;
				transform.localPosition = Vector3.zero;
			}
		}
	}
}
