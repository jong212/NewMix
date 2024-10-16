using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Character Specs")]
public class CharacterSpecs : ScriptableObject
{
	[field: SerializeField] public float MovementSpeed { get; private set; }
	[field: SerializeField] public float Reach { get; private set; }
	[field: SerializeField] public float ThrowForce { get; private set; }
	[field: SerializeField] public float ThrowArc { get; private set; }
}
