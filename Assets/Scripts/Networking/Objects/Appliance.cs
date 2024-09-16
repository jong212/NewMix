using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Appliance : WorkSurface
{
	public ApplianceData Data;

	public override void FixedUpdateNetwork()
	{
		base.FixedUpdateNetwork();

		if (!Object.HasStateAuthority) return;

		if (ItemOnTop)
		{
			if (ItemOnTop.TryGetComponent(out FoodProcessor processor))
			{
				if (Data.Processor == processor.Data)
				{
					if (processor.TryGetComponent(out Interactable interactable))
					{
						interactable.UseInteract(null);
					}
				}
			}
		}
	}
}
