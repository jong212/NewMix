using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderGroup : NetworkBehaviour
{
	public Collider[] colliders;
	[Networked, OnChangedRender(nameof(CollidersEnabledChanged))] public bool CollidersEnabled { get; set; }

	private void OnValidate()
	{
		colliders = GetComponentsInChildren<Collider>();
	}

	public override void Spawned()
	{
		CollidersEnabledChanged();
	}

	void CollidersEnabledChanged()
	{
		foreach (Collider c in colliders) c.enabled = CollidersEnabled;
	}
}
