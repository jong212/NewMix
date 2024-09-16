using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnpointManager : MonoBehaviour
{
	[SerializeField] private LayerMask obstableMask;
	[SerializeField] private Spawnpoint[] spawnpoints;
	[SerializeField] private Spawnpoint fallback;

	public static SpawnpointManager Instance { get; set; }

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		if (Instance == this) Instance = null;
	}

	public static bool GetSpawnpoint(out Vector3 location, out Quaternion orientation)
	{
		if (Instance == null)
		{
			location = default;
			orientation = default;
			return false;
		}

		for (int i = 0; i < Instance.spawnpoints.Length; i++)
		{
			if (Instance.spawnpoints[i].HasClearance(Instance.obstableMask))
			{
				location = Instance.spawnpoints[i].transform.position;
				orientation = Instance.spawnpoints[i].transform.rotation;
				return true;
			}
		}

		if (Instance.fallback)
		{
			location = Instance.fallback.transform.position;
			orientation = Instance.fallback.transform.rotation;
			return true;
		}

		location = default;
		orientation = default;
		return false;
	}
}
