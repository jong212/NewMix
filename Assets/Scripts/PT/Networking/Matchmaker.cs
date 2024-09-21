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
                // TODO gameplayHUD 콜백함수로 사용하면 되어서 나중에 MixMaster UI 같은거 세팅시키면 좋을듯 
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

		// 요약
        // 아직 게임 씬이 로드되기 전의 상태입니다. 
        // `Instantiate`로 `runnerPrefab`을 인스턴스화하면, 아직 Game 씬이 로드되지 않았기 때문에
        // `Prototype Runner(Clone)`이라는 이름의 오브젝트가 생성되며, 이 오브젝트는 `DontDestroyOnLoad`로 설정됩니다.
        // `networkEvents.OnSceneLoadDone.AddListener(SpawnManager);` 이 줄은 Game 씬이 로드가 완료되었을 때, 즉 씬 로드가 완료되는 시점에 `SpawnManager` 함수를 실행하도록 이벤트 리스너를 등록합니다.
        // 또한, `Runner` 인스턴스에는 `PlayerSpawner` 컴포넌트가 포함되어 있으며, 그 스크립트에서 `PlayerJoined` 함수가 `task = Runner.StartGame` 작업이 완료되었을 때 자동으로 호출됩니다.        
        Runner = Instantiate(runnerPrefab);
     
        NetworkEvents networkEvents = Runner.GetComponent<NetworkEvents>();

        void SpawnManager(NetworkRunner runner)
        {
            if (Runner.IsSharedModeMasterClient) runner.Spawn(managerPrefab);
            networkEvents.OnSceneLoadDone.RemoveListener(SpawnManager);
        }
        // networkEvents.OnSceneLoadDone.AddListener(SpawnManager); 는 Runner.LoadScene(sessionCode); 에서 로드된 씬이 완전히 로드된 후 SpawnManager가 호출되도록 설정하는 코드
        networkEvents.OnSceneLoadDone.AddListener(SpawnManager);

        Runner.AddCallbacks(this);

        // Runner.StartGame이 비동기로 실행되어, 게임 세션을 시작하는 작업이 백그라운드에서 진행됩니다.
        Task<StartGameResult> task = Runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = sessionCode,
            SceneManager = Runner.GetComponent<INetworkSceneManager>(),
            ObjectProvider = Runner.GetComponent<INetworkObjectProvider>(),
        });

        // task 작업이 완료될 때까지 현재 코루틴이 대기 상태로 들어가게 됩니다. 이 대기는 병렬로 실행 중인 비동기 작업(task)의 완료를 기다리는 것이지, 클라이언트의 전체 로직을 멈추는 것이 아닙니다.
        // 코루틴이 대기 중일 때도, Unity의 메인 게임 루프는 계속 실행됩니다.이는 프레임 업데이트, 물리 연산, 다른 코루틴의 실행 등이 정상적으로 이루어진다는 의미입니다. 예를 들어, while (!task.IsCompleted) yield return null; 에서 코루틴이 대기하는 동안에도, 게임의 다른 부분(예: 플레이어 움직임, UI 업데이트 등)은 정상적으로 진행됩니다.
        while (!task.IsCompleted) yield return null;

        StartGameResult result = task.Result;

        if (result.Ok)
        {
            successCallback?.Invoke();
            if (Runner.IsSharedModeMasterClient) Runner.LoadScene(sessionCode); // 세션 이름을 그대로 씬 이름으로 사용
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