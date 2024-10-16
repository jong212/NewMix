using System.Linq;
using UnityEngine;

public class DeliverOrderInteraction : Interaction
{
	public override bool TryInteract(Character interactor)
	{
		if (interactor.HeldItem == null) return false;
		if (!interactor.HeldItem.TryGetComponent(out FoodContainer playerContainer)) return false;
		if (!interactor.HeldItem.TryGetComponent(out Item item)) return false;
		if (!item.Deliverable) return false;

		LogInteraction();

		Debug.Log(string.Join(", ", playerContainer.Ingredients.Select(i => i.Data.DisplayName)));

		//GameManager.instance.Rpc_SubmitOrder(playerContainer);
		return true;
	}
}
