using System.Linq;
using UnityEngine;
using Fusion;

public class AuthorityHandler : NetworkBehaviour, IStateAuthorityChanged
{
	[Networked] System.Guid Guid { get; set; }

	private bool isAuthorityChanging = false;

	float timeRequested = 0;
	System.Action onAuthorized = null;
	System.Action onUnauthorized = null;

	public override void Spawned()
	{
		if (Object.HasStateAuthority) Guid = System.Guid.NewGuid();
	}

	public void RequestAuthority(System.Action onAuthorized = null, System.Action onUnauthorized = null)
	{
		timeRequested = Runner.SimulationTime;
		this.onAuthorized = onAuthorized;
		this.onUnauthorized = onUnauthorized;

		if (!Object.StateAuthority.IsNone && Runner.ActivePlayers.FirstOrDefault(p => p == Object.StateAuthority) != default)
		{
			Rpc_RequestAuthority(Object.StateAuthority, GetHierarchyState());
		}
		else
		{
			Rpc_Authorized(Runner.LocalPlayer);
		}
	}

	private System.Guid GetHierarchyState()
	{
		byte[] g1 = new byte[16];
		foreach (var authHandler in GetComponentsInChildren<AuthorityHandler>())
		{
			byte[] g2 = authHandler.Guid.ToByteArray();
			for (var i = 0; i < 16; i++)
			{
				g1[i] ^= g2[i];
			}
		}
		return new System.Guid(g1);
	}

	[Rpc(RpcSources.All, RpcTargets.All)]
	private void Rpc_RequestAuthority([RpcTarget] PlayerRef player, System.Guid expectedState, RpcInfo info = default)
	{
		if (Object.HasStateAuthority && !isAuthorityChanging && expectedState.Equals(GetHierarchyState()))
		{
			if (info.IsInvokeLocal)
			{
				onAuthorized?.Invoke();
				onAuthorized = null;
				onUnauthorized = null;
				timeRequested = 0;
			}
			else
			{
				isAuthorityChanging = true;
				Rpc_Authorized(info.Source);
				Log($"authorizing {info.Source} for {gameObject.name}", gameObject);
			}
		}
		else
		{
			Rpc_NotAuthorized(info.Source);
		}
	}

	[Rpc(RpcSources.All, RpcTargets.All)]
	private void Rpc_Authorized([RpcTarget] PlayerRef player)
	{
		Log("authorized...");
		AuthorityHandler[] authHandlers = GetComponentsInChildren<AuthorityHandler>();
		foreach (AuthorityHandler authHandler in authHandlers)
		{
			Log(authHandler.gameObject.name, gameObject);
			authHandler.Object.RequestStateAuthority();
		}
	}

	[Rpc(RpcSources.All, RpcTargets.All)]
	private void Rpc_NotAuthorized([RpcTarget] PlayerRef player)
	{
		Log("not authorized");
		onUnauthorized?.Invoke();
		onAuthorized = null;
		onUnauthorized = null;
	}

	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	public void Rpc_Despawn()
	{
		Runner.Despawn(Object);
	}

	private void Log(string message, Object ctx = null) => Debug.Log(string.Join('\n', message.Split('\n').Select(s => $"<color=#8080ff>{s}</color>")), ctx);

	public void StateAuthorityChanged()
	{
		if (isAuthorityChanging)
		{
			Log("authority changed.");
			isAuthorityChanging = false;
		}
	}

	public override void FixedUpdateNetwork()
	{
		if (onAuthorized != null)
		{
			if (GetComponentsInChildren<AuthorityHandler>().All(h => h.Object.HasStateAuthority))
			{
				Log("invoking authority action");
				onAuthorized();
				onAuthorized = null;
				onUnauthorized = null;
				timeRequested = 0;
			}
		}
	}

	public override void Render()
	{
		if (timeRequested > 0 && Runner.SimulationTime - timeRequested > 2)
		{
			Debug.LogWarning("timed out");
			timeRequested = 0;
			onUnauthorized?.Invoke();
			onAuthorized = null;
			onUnauthorized = null;
		}
	}
}