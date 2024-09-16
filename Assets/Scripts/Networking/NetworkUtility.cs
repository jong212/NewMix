using Fusion;
using System.Collections.Generic;

public static class NetworkUtility
{
	public static IEnumerable<T> Enumerable<T>(this NetworkLinkedListReadOnly<T> list)
	{
		var ret = new T[list.Count];
		for (int i = 0; i < list.Count; i++)
			ret[i] = list[i];
		return ret;
	}
}
