using System.Collections.Generic;
using UnityEngine;

public static class ProcessGraph
{
	private static readonly Dictionary<FoodProcessorData, Dictionary<IngredientData, ProcessGraphData>> Processes = new();

	static ProcessGraph()
	{
		foreach (FoodProcess process in Resources.LoadAll<FoodProcess>(""))
		{
			if (!Processes.ContainsKey(process.Processor))
			{
				Processes.Add(process.Processor, new Dictionary<IngredientData, ProcessGraphData>());
			}

			if (Processes[process.Processor].ContainsKey(process.IngredientIn))
			{
				Debug.LogWarning($"Duplicate food process entry '{process.IngredientIn}' for {process.Processor}");
			}
			else
			{
				Processes[process.Processor].Add(process.IngredientIn, new ProcessGraphData(process.IngredientOut, process.TicksToComplete));
			}
		}
	}

	/// <summary>
	/// Ensures that the class constructor has ran.
	/// Call before the data is needed to avoid lag.
	/// </summary>
	public static void Prepare() { }

	public static bool IsCompatible(FoodProcessorData processor, IngredientData ingredient)
	{
		if (!Processes.TryGetValue(processor, out var processes)) return false;
		return processes.ContainsKey(ingredient);
	}

	public static bool GetProcessData(FoodProcessorData processor, IngredientData ingredient, out ProcessGraphData data)
	{
		data = default;
		if (!Processes.TryGetValue(processor, out var processes)) return false;
		return processes.TryGetValue(ingredient, out data);
	}

	public readonly struct ProcessGraphData
	{
		public readonly IngredientData Result;
		public readonly int TicksToComplete;

		public ProcessGraphData(IngredientData result, int ticksToComplete)
		{
			Result = result;
			TicksToComplete = ticksToComplete;
		}
	}
}
