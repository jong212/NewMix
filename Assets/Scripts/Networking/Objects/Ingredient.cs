using UnityEngine;
using Fusion;

public class Ingredient : NetworkBehaviour
{
	public IngredientData Data;
	[Networked] public float ProcessedAmount { get; set; }
	[field: SerializeField] public GameObject Visual { get; private set; }
	[SerializeField] private GameObject defaultVisual;
	[SerializeField] private GameObject processingVisual;

	private ChangeDetector _changes;

	public override void Spawned()
	{
		_changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
	}

	public override void Render()
	{
		CheckForChanges();
	}

	private void CheckForChanges()
	{
		foreach (string change in _changes.DetectChanges(this, out NetworkBehaviourBuffer previousBuffer, out NetworkBehaviourBuffer currentBuffer))
		{
			switch (change)
			{
				case nameof(ProcessedAmount):
					var reader = GetPropertyReader<float>(nameof(ProcessedAmount));
					var previous = reader.Read(previousBuffer);
					var current = reader.Read(currentBuffer);
					ProcessedAmountChanged(previous, current);
					break;
			}
		}
	}

	private void ProcessedAmountChanged(float oldValue, float newValue)
	{
		if (processingVisual == null) return;

		if (oldValue <= 0 && newValue > 0)
		{
			defaultVisual.SetActive(false);
			processingVisual.SetActive(true);
		}
	}
}