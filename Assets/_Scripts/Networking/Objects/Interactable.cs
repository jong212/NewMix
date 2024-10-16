using System.Linq;
using UnityEngine;

public class Interactable : MonoBehaviour
{
	public Interaction[] GrabInteractions;
	public Interaction[] UseInteractions;

	public bool GrabInteract(Character interactor)
	{
		return GrabInteractions.Any(interaction => interaction.TryInteract(interactor));
	}

	public bool UseInteract(Character interactor)
	{
		return UseInteractions.Any(interaction => interaction.TryInteract(interactor));
	}
}