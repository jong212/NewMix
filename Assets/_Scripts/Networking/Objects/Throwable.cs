using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[RequireComponent(typeof(Rigidbody), typeof(ColliderGroup))]
public class Throwable : NetworkBehaviour
{
	private Rigidbody _rb;
	public Rigidbody Rigidbody => _rb ??= GetComponent<Rigidbody>();

	private ColliderGroup _colliders;
	public ColliderGroup Colliders => _colliders ??= GetComponent<ColliderGroup>();

	private void OnCollisionEnter(Collision collision)
	{
		if (!HasStateAuthority) return;
		if (Rigidbody.isKinematic) return;

		if (collision.gameObject.GetComponentInParent<WorkSurface>() is WorkSurface surf)
		{
			if (surf.ItemOnTop == null)
			{
				surf.GetComponent<AuthorityHandler>().RequestAuthority(() =>
				{
					Rigidbody.isKinematic = true;
					transform.SetPositionAndRotation(surf.SurfacePoint.position, surf.SurfacePoint.rotation);
					transform.SetParent(surf.transform, true);
					surf.ItemOnTop = GetComponent<Item>();
				});
			}
		}
		else if (collision.gameObject.GetComponentInParent<VoidIngredientInteraction>())
		{
			Runner.Despawn(Object);
		}
	}

	public void Throw(Vector3 force)
	{
		transform.SetParent(null);
		Rigidbody.isKinematic = false;
		Rigidbody.AddForce(force, ForceMode.Impulse);
		Colliders.CollidersEnabled = true;
	}
}
