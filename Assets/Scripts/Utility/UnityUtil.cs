using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityUtil
{
	public static void DestroyChildren(this Transform tf)
	{
		foreach(Transform child in tf)
		{
			Object.Destroy(child.gameObject);
		}
	}

	public static void DestroyChildren(this Transform tf, int startIndex)
	{
		int i = 0;
		foreach (Transform child in tf)
		{
			if (i++ < startIndex) continue;
			Object.Destroy(child.gameObject);
		}
	}

	public static void DestroyChildrenWithComponent<T>(this Transform tf) where T : Component
	{
		foreach(Transform child in tf)
		{
			if (child.TryGetComponent(out T _))
				Object.Destroy(child.gameObject);
		}
	}

	public static void DestroyChildrenWithoutComponent<T>(this Transform tf) where T : Component
	{
		foreach (Transform child in tf)
		{
			if (!child.TryGetComponent(out T _))
				Object.Destroy(child.gameObject);
		}
	}

	public static T GetComponentTopmost<T>(this Component c)
	{
		T[] arr = c.GetComponentsInParent<T>();
		if (arr == null || arr.Length == 0) return default;
		return arr[arr.Length - 1];
	}
}
