using UnityEngine;

public class Spawnpoint : MonoBehaviour
{
	[SerializeField] private float clearance;

	private readonly Collider[] resultArray = new Collider[1];

	public bool HasClearance(int layerMask)
	{
		bool hasClearance = Physics.OverlapSphereNonAlloc(transform.position, clearance, resultArray, layerMask, QueryTriggerInteraction.Ignore) == 0;
		resultArray[0] = null;
		return hasClearance;
	}

	private void OnDrawGizmos()
	{
		Gizmos.matrix = transform.localToWorldMatrix;

		Gizmos.color = Color.gray;
		Gizmos.DrawWireSphere(Vector3.zero, clearance);

		Gizmos.color = Color.red;
		Gizmos.DrawRay(Vector3.zero, Vector3.right * clearance);

		Gizmos.color = Color.green;
		Gizmos.DrawRay(Vector3.zero, Vector3.up * clearance);

		Gizmos.color = Color.blue;
		Gizmos.DrawRay(Vector3.zero, Vector3.forward * clearance);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = transform.localToWorldMatrix;

		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(Vector3.zero, clearance);
	}
}
