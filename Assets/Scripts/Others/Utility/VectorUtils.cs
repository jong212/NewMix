using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorUtils
{
	public static Vector3 Swizzle(this Vector3 v, int x, int y, int z)
	{
		return new Vector3(v[x], v[y], v[z]);
	}
}
