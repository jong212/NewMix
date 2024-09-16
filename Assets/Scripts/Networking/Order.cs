using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using JetBrains.Annotations;

public struct Order : INetworkStruct
{
	[Networked, Capacity(5)] public NetworkArray<short> IngredientKeys => default;
	public int TickCreated;
	public int Id;
	
	public readonly bool IsValid => TickCreated > 0;

	public static implicit operator string(Order o) => o.ToString();
	public override string ToString()
	{
		return $"#{Id} @ {TickCreated} : {string.Join(",", IngredientKeys)}";
	}
}
