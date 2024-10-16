using UnityEngine;
using Fusion;

public class Item : NetworkBehaviour
{
	[SerializeField] bool _trashable;
	[SerializeField] bool _deliverable;
	public bool Trashable => _trashable;
	public bool Deliverable => _deliverable;
}