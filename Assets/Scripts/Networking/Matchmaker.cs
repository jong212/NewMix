using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;

public class Matchmaker : MonoBehaviour, INetworkRunnerCallbacks
{
	public static Matchmaker Instance { get; private set; }

	[SerializeField, ScenePath] string gameScene;
	public NetworkRunner runnerPrefab;
	public NetworkObject managerPrefab;

	public NetworkRunner Runner { get; private set; }

	string _roomCode = null;

	public void SetRoomCode(string code)
	{
		_roomCode = code;
	}

	private void Awake()
	{
		if (Instance != null) { Destroy(gameObject); return; }
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	private void OnDestroy()
	{
		if (Instance == this) Instance = null;
	}

	public void TryConnectShared()
	{
		TryConnectSharedSession(
			string.IsNullOrWhiteSpace(_roomCode) ? $"FoodFusion{Random.Range(1000, 9999)}" : _roomCode,
			() =>
			{
				UIScreen.Focus(InterfaceManager.instance.gameplayHUD);
			});
	}

	public void TryConnectSharedSession(string sessionCode, System.Action successCallback = null)
	{
		StartCoroutine(ConnectSharedSessionRoutine(sessionCode, successCallback));
	}

	IEnumerator ConnectSharedSessionRoutine(string sessionCode, System.Action successCallback)
	{
		if (Runner) Runner.Shutdown();
		Runner = Instantiate(runnerPrefab);

		NetworkEvents networkEvents = Runner.GetComponent<NetworkEvents>();

		void SpawnManager(NetworkRunner runner)
		{
			if (Runner.IsSharedModeMasterClient) runner.Spawn(managerPrefab);
			networkEvents.OnSceneLoadDone.RemoveListener(SpawnManager);
		}

		networkEvents.OnSceneLoadDone.AddListener(SpawnManager);

		Runner.AddCallbacks(this);

		Task<StartGameResult> task = Runner.StartGame(new StartGameArgs()
		{
			GameMode = GameMode.Shared,
			SessionName = sessionCode,
			SceneManager = Runner.GetComponent<INetworkSceneManager>(),
			ObjectProvider = Runner.GetComponent<INetworkObjectProvider>(),
		});

		while (!task.IsCompleted) yield return null;

		StartGameResult result = task.Result;

		if (result.Ok)
		{
			successCallback?.Invoke();
			if (Runner.IsSharedModeMasterClient) Runner.LoadScene(gameScene);
		}
		else
		{
			Debug.LogWarning(result.ShutdownReason);
			DisconnectUI.OnShutdown(result.ShutdownReason);
		}
	}

	// -------

	public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
	{
		Runner = null;
		if (shutdownReason == ShutdownReason.Ok)
		{
			SceneManager.LoadScene("Menu");
            UIScreen.activeScreen.BackTo(InterfaceManager.instance.kitchenConnectScreen);
		}
		else
		{
			Debug.LogWarning(shutdownReason);
			DisconnectUI.OnShutdown(shutdownReason);
		}
	}

	#region INetworkRunnerCallbacks
	public void OnConnectedToServer(NetworkRunner runner) { }
	public void OnConnectFailed(NetworkRunner runner, Fusion.Sockets.NetAddress remoteAddress, Fusion.Sockets.NetConnectFailedReason reason) { }
	public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
	public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
	public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
	public void OnInput(NetworkRunner runner, NetworkInput input) { }
	public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
	public void OnSceneLoadStart(NetworkRunner runner) { }
	public void OnSceneLoadDone(NetworkRunner runner) { }
	public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
	public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
	public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
	public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
	public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
	public void OnDisconnectedFromServer(NetworkRunner runner, Fusion.Sockets.NetDisconnectReason reason) { }
	public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, Fusion.Sockets.ReliableKey key, System.ArraySegment<byte> data) { }
	public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, Fusion.Sockets.ReliableKey key, float progress) { }
	#endregion
}