using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;

[DefaultExecutionOrder(-200)]
public class GameManager : NetworkBehaviour, IStateAuthorityChanged, IPlayerLeft
{
	public static event System.Action<byte> OnReservedPlayerVisualsChanged;

	[Networked, Capacity(6), UnitySerializeField] public NetworkLinkedList<Order> OrderList => default;
	[Networked] public int OrdersSpawned { get; set; }
	[Networked] public TickTimer OrderTimer { get; set; }

	[SerializeField] private float orderInterval = 20;
	[SerializeField] private float orderIntervalFilled = 10;
	[SerializeField] private float orderLifetime = 180;

	private Dictionary<int, FoodOrderItemUI> OrderUIs { get; } = new();

	private ChangeDetector _changes;

	#region Singleton

	public static GameManager instance;

	private void Awake()
	{
		if (instance)
		{
			Debug.LogWarning("Instance already exists!");
			Destroy(gameObject);
		}
		else
		{
			instance = this;
		}
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			CleanupOrderUIs();
			instance = null;
		}
	}

	#endregion

	public override void Spawned()
	{
		InterfaceManager.instance.roomNameText.text = Runner.SessionInfo.Name;

		if (Object.HasStateAuthority) StartCoroutine(PrepareOrderTimer());

		_changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

		OrdersAdded(OrderList);
	}

	private IEnumerator PrepareOrderTimer()
	{
		// Wait until the master client has spawned their player before starting the game loop
		yield return new WaitUntil(() => Runner.TryGetPlayerObject(PlayerRef.MasterClient, out _));
		OrderTimer = TickTimer.CreateFromSeconds(Runner, 1);
	}

	public override void Render()
	{
		CheckForChanges();
		RepaintOrders();
	}

	public override void FixedUpdateNetwork()
	{
		// If the timer is up, create a new order, if possible.
		// Set a new timer based on whether the orders are full or not.
		if (OrderTimer.Expired(Runner))
		{
			if (CreateOrder())
				OrderTimer = TickTimer.CreateFromSeconds(Runner, orderInterval);
			else
				OrderTimer = TickTimer.CreateFromSeconds(Runner, orderIntervalFilled);
		}

		// Evaluate the current orders and remove any which have expired.
		foreach (Order order in OrderList)
		{
			if (order.IsValid)
			{
				float time = ((int)Runner.Tick - order.TickCreated) * Runner.DeltaTime / orderLifetime;
				if (time >= 1)
				{
					RemoveOrder(order);
					break;
				}
			}
		}
	}

	private void CheckForChanges()
	{
		foreach (string change in _changes.DetectChanges(this, out NetworkBehaviourBuffer previousBuffer, out NetworkBehaviourBuffer currentBuffer))
		{
			switch (change)
			{
				case nameof(OrderList):
					var reader = GetLinkListReader<Order>(nameof(OrderList));
					var previous = reader.Read(previousBuffer);
					var current = reader.Read(currentBuffer);
					OrdersChanged(previous, current);
					break;
			}
		}
	}

	#region Player Visuals
	public void ReservedPlayerVisualsChanged()
	{
		byte mask = (byte)Runner.ActivePlayers
			.Select(p => Runner.TryGetPlayerObject(p, out NetworkObject pObj) ? pObj : null)
			.Where(p => p != null)
			.Select(o => o.GetBehaviour<Character>())
			.Select(c => 1 << c.Visual)
			.Sum();

		Debug.Log("Visuals reserved mask: " + System.Convert.ToString(mask, 2).PadLeft(8, '0'));

		OnReservedPlayerVisualsChanged?.Invoke(mask);
	}

	public bool IsPlayerVisualAvailable(int index)
	{
		return Runner.ActivePlayers
			.Select(p => Runner.TryGetPlayerObject(p, out NetworkObject pObj) ? pObj : null)
			.Where(p => p != null)
			.Select(o => o.GetBehaviour<Character>())
			.Any(c => c.Visual == index) == false;
	}
	#endregion

	#region Orders
	private void RepaintOrders()
	{
		int t = Runner.Tick;
		foreach (Order order in OrderList)
		{
			if (order.IsValid)
			{
				float time = (t - order.TickCreated) * Runner.DeltaTime / orderLifetime;
				if (OrderUIs.TryGetValue(order.Id, out var ui))
				{
					ui.Paint(1 - time);
				}
			}
		}
	}

	private void OrdersChanged(NetworkLinkedListReadOnly<Order> prevOrders, NetworkLinkedListReadOnly<Order> currOrders)
	{
		IEnumerable<Order> prevEnum = prevOrders.Enumerable();
		IEnumerable<Order> currEnum = currOrders.Enumerable();

		IEnumerable<Order> added = currEnum.Except(prevEnum);
		IEnumerable<Order> removed = prevEnum.Except(currEnum);

		foreach (var rem in removed)
		{
			float time = ((int)Runner.Tick - rem.TickCreated) * Runner.DeltaTime / orderLifetime;
			if (time < 1)
			{
				AudioManager.Play("successUI", AudioManager.MixerTarget.SFX);
				OrderUIs[rem.Id].FlashComplete();
			}
			OrderUIs[rem.Id].Expire();
			OrderUIs.Remove(rem.Id);
		}

		OrdersAdded(added);
	}

	private void OrdersAdded(IEnumerable<Order> added)
	{
		foreach (var add in added)
		{
			var ingredients =
				add.IngredientKeys
				.Select(k => ResourcesManager.instance.ingredientBank.GetValue<IngredientData>(k))
				.Where(v => v != null)
				.ToArray();

			if (ingredients.Length > 0)
			{
				FoodOrderItemUI orderUI = Instantiate(
					ResourcesManager.instance.foodOrderUIPrefab,
					InterfaceManager.instance.foodOrderItemHolder);
				orderUI.SetOrder(ingredients);
				orderUI.orderNumber.text = $"{add.Id}";

				OrderUIs.Add(add.Id, orderUI);
			}
		}
	}

	private void RemoveOrder(Order order)
	{
		OrderList.Remove(order);
	}

	public void CleanupOrderUIs()
	{
		foreach (var ui in OrderUIs.Values)
		{
			if (ui != null) Destroy(ui.gameObject);
		}
		OrderUIs.Clear();
	}

	public bool CreateOrder()
	{
		// Is the order list full?
		if (OrderList.Count >= OrderList.Capacity) return false;

		OrdersSpawned++;

		IngredientData[] orderIngredients = OrderMenu.GetOrder();
		short[] ingredientKeys = orderIngredients.Select(i => ResourcesManager.instance.ingredientBank.GetKey(i)).ToArray();

		Order order = new()
		{
			TickCreated = Runner.Tick,
			Id = OrdersSpawned
		};
		order.IngredientKeys.CopyFrom(ingredientKeys, 0, ingredientKeys.Length);

		OrderList.Add(order);
		return true;
	}

	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	public void Rpc_SubmitOrder(FoodContainer container)
	{
		HashSet<IngredientData> containerIngredientData = container.Ingredients.Select(i => i.Data).ToHashSet();

		container.GetBehaviour<AuthorityHandler>().Rpc_Despawn();

		// Check each order to see if any match what was submitted
		foreach (Order order in OrderList)
		{
			IngredientData[] orderIngredients = order.IngredientKeys
				.Where(i => i != 0)
				.Select(i => ResourcesManager.instance.ingredientBank.GetValue<IngredientData>(i))
				.ToArray();

			if (containerIngredientData.SetEquals(orderIngredients))
			{
				RemoveOrder(order);
				return;
			}
		}

		// No match, give the players feedback that a mistake was made
		Rpc_OrderBad();
	}

	[Rpc(RpcSources.StateAuthority, RpcTargets.All, InvokeLocal = true)]
	public void Rpc_OrderBad()
	{
		AudioManager.Play("errorUI", AudioManager.MixerTarget.SFX);
		foreach (FoodOrderItemUI ui in OrderUIs.Values) ui.FlashError();
	}
	#endregion

	public void StateAuthorityChanged()
	{
		if (Runner.IsSharedModeMasterClient)
		{
			CleanupLeftPlayers();
		}
	}

	public void PlayerLeft(PlayerRef player)
	{
		if (Runner.IsSharedModeMasterClient)
		{
			CleanupLeftPlayers();
		}
	}

	void CleanupLeftPlayers()
	{
		// Characters need special handling when their player disconnects.
		// If they were holding an item, the item needs to be reparented before the character object is despawned.

		Character[] objs = FindObjectsOfType<Character>()
			.Where(c => !Runner.ActivePlayers.Contains(c.Object.StateAuthority))
			.ToArray();

		foreach (Character c in objs)
		{
			if (c.Object.IsValid)
			{
				c.GetComponent<AuthorityHandler>().RequestAuthority(() =>
				{
					Item item = c.HeldItem;
					if (item)
					{
						// find the nearest empty work surface
						WorkSurface surf = FindObjectsOfType<WorkSurface>()
							.OrderBy(w => Vector2.Distance(
								new Vector2(c.transform.position.x, c.transform.position.z),
								new Vector2(w.transform.position.x, w.transform.position.z)))
							.FirstOrDefault(w => w.ItemOnTop == null);

						if (surf)
						{
							surf.GetComponent<AuthorityHandler>().RequestAuthority(() =>
							{
								surf.ItemOnTop = item;
								item.transform.SetPositionAndRotation(surf.SurfacePoint.position, surf.SurfacePoint.rotation);
								item.transform.SetParent(surf.Object.transform, true);
								Runner.Despawn(c.Object);
							});
						}
						else
						{
							Runner.Despawn(item.Object);
							Runner.Despawn(c.Object);
						}
					}
					else Runner.Despawn(c.Object);
				});
			}
		}
	}
}