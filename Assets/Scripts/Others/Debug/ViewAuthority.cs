using UnityEngine;
using Fusion;
using System.Linq;
using Drawing.Glint;

/// <summary>
/// Visualizes the authority and relations of NetworkObjects.
/// </summary>
public class ViewAuthority : MonoBehaviour
{
	private void LateUpdate()
	{
		NetworkRunner runner = NetworkRunner.Instances.FirstOrDefault();
		if (runner == null) return;

		Camera cam = Camera.main;
		var objs = FindObjectsOfType<NetworkObject>();

		foreach ( NetworkObject obj in objs)
		{
			Color c = obj.StateAuthority == PlayerRef.None ? 
				Color.white : 
				runner.TryGetPlayerObject(obj.StateAuthority, out NetworkObject nObj) && nObj.TryGetBehaviour(out Character character) ? 
				character.CharacterVisual.color : 
				Color.gray;

			var rens = obj.GetComponentsInChildren<Renderer>();

			Bounds b = new (obj.transform.position, Vector3.zero);
			foreach (Renderer ren in rens)
			{
				b.Encapsulate(ren.bounds);
			}

			if (b.size.sqrMagnitude > 0)
			{
				Vector3[] corners = new Vector3[]
				{
					cam.WorldToScreenPoint(new Vector3(b.min.x, b.min.y, b.min.z)),
					cam.WorldToScreenPoint(new Vector3(b.min.x, b.min.y, b.max.z)),
					cam.WorldToScreenPoint(new Vector3(b.min.x, b.max.y, b.min.z)),
					cam.WorldToScreenPoint(new Vector3(b.min.x, b.max.y, b.max.z)),
					cam.WorldToScreenPoint(new Vector3(b.max.x, b.min.y, b.min.z)),
					cam.WorldToScreenPoint(new Vector3(b.max.x, b.min.y, b.max.z)),
					cam.WorldToScreenPoint(new Vector3(b.max.x, b.max.y, b.min.z)),
					cam.WorldToScreenPoint(new Vector3(b.max.x, b.max.y, b.max.z)),
				};

				Vector3 min = new(
					corners.Select(v => v.x).Min(),
					corners.Select(v => v.y).Min());

				Vector3 max = new(
					corners.Select(v => v.x).Max(),
					corners.Select(v => v.y).Max());

				Glint.AddCommand(new Drawing.Glint.GLCommand(Drawing.Glint.DrawMode.LineStrip, Color.black,
					new Vector3(min.x - 1, min.y - 1),
					new Vector3(min.x - 1, max.y + 1),
					new Vector3(max.x + 1, max.y + 1),
					new Vector3(max.x + 1, min.y - 1),
					new Vector3(min.x - 1, min.y - 1)
					));

				Glint.AddCommand(new Drawing.Glint.GLCommand(Drawing.Glint.DrawMode.LineStrip, c,
					new Vector3(min.x, min.y),
					new Vector3(min.x, max.y),
					new Vector3(max.x, max.y),
					new Vector3(max.x, min.y),
					new Vector3(min.x, min.y)
					));

				if (obj.TryGetBehaviour(out FoodContainer container))
				{
					foreach (var ing in container.Ingredients)
					{
						Vector3 pos_a = (Vector2)min;
						Vector3 pos_b = (Vector2)cam.WorldToScreenPoint(ing.transform.position);

						Glint.AddCommand(new Drawing.Glint.GLCommand(Drawing.Glint.DrawMode.Lines, Color.black, pos_a + Vector3.down, pos_b + Vector3.down));
						Glint.AddCommand(new Drawing.Glint.GLCommand(Drawing.Glint.DrawMode.Lines, c, pos_a, pos_b));
					}
				}

				if (obj.TryGetBehaviour(out WorkSurface surf) && surf.ItemOnTop)
				{
					Vector3 pos_a = (Vector2)min;
					Vector3 pos_b = (Vector2)cam.WorldToScreenPoint(surf.ItemOnTop.transform.position);

					Glint.AddCommand(new Drawing.Glint.GLCommand(Drawing.Glint.DrawMode.Lines, Color.black, pos_a + Vector3.down, pos_b + Vector3.down));
					Glint.AddCommand(new Drawing.Glint.GLCommand(Drawing.Glint.DrawMode.Lines, Color.white, pos_a, pos_b));
				}
			}

			
		}
	}
}
