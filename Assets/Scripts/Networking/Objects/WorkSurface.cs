using UnityEngine;
using Fusion;

[DisallowMultipleComponent]
public class WorkSurface : NetworkBehaviour
{
	public Transform SurfacePoint;
	[Networked] public Item ItemOnTop { get; set; }
}