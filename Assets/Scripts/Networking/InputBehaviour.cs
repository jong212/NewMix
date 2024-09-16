using System.Linq;
using UnityEngine;
using Fusion;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
#endif

public class InputBehaviour : Fusion.Behaviour, INetworkRunnerCallbacks
{
#if ENABLE_INPUT_SYSTEM
	bool isReady;
	InputAction moveXInput;
	InputAction moveYInput;
	InputAction grabInput;
	InputAction useInput;

	System.IDisposable setupHandle;

	
	private void Start()
	{
		Log("Listening for button press...");
		setupHandle = InputSystem.onAnyButtonPress.Call(SetupInputs);

		void SetupInputs(InputControl inputControl)
		{
			var device = inputControl.device;
			if (device is Gamepad gp)
			{
				Log("Got gamepad input.");

				moveXInput = new InputAction("Move X", InputActionType.Value, device["leftStick/x"].path);
				moveYInput = new InputAction("Move Y", InputActionType.Value, device["leftStick/y"].path);
				grabInput = new InputAction("Grab", InputActionType.Button, gp.buttonSouth.path);
				useInput = new InputAction("Use", InputActionType.Button, gp.buttonWest.path);

				Finalize();
			}
			else if (device is Keyboard kb)
			{
				Log("Got keyboard input.");

				moveXInput = new InputAction("Move X");
				moveXInput.AddCompositeBinding("Axis").With("Positive", kb.dKey.path).With("Negative", kb.aKey.path);
				moveYInput = new InputAction("Move Y");
				moveYInput.AddCompositeBinding("Axis").With("Positive", kb.wKey.path).With("Negative", kb.sKey.path);
				grabInput = new InputAction("Grab", InputActionType.Button, kb.eKey.path);
				useInput = new InputAction("Use", InputActionType.Button, kb.fKey.path);

				Finalize();
			}

			void Finalize()
			{
				Log($"Initialized controls:\n{moveXInput}\n{moveYInput}\n{grabInput}\n{useInput}");

				moveXInput.Enable();
				moveYInput.Enable();
				grabInput.Enable();
				useInput.Enable();

				isReady = true;
				setupHandle.Dispose();
			}
		}
		
		void Log(string message) => Debug.Log(string.Join('\n', message.Split('\n').Select(s => $"<color=#ff8080>{s}</color>")));
	}
#endif

	public void OnInput(NetworkRunner runner, NetworkInput input)
	{
		PlayerInput i = new();

#if ENABLE_INPUT_SYSTEM
		if (isReady)
		{
			Vector2 move = new(moveXInput.ReadValue<float>(), moveYInput.ReadValue<float>());
			i.MoveAmount = move.magnitude;
			i.MoveDirection = Mathf.Atan2(move.x, move.y) * Mathf.Rad2Deg;
			i.Grab = grabInput.IsPressed();
			i.Use = useInput.IsPressed();
		}
#else
		Vector2 move = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		i.MoveAmount = move.magnitude;
		i.MoveDirection = Mathf.Atan2(move.x, move.y) * Mathf.Rad2Deg;
		i.Grab = Input.GetButton("Grab");
		i.Use = Input.GetButton("Use");
#endif

		input.Set(i);
	}

	#region INetworkRunnerCallbacks
	public void OnConnectedToServer(NetworkRunner runner) { }
	public void OnConnectFailed(NetworkRunner runner, Fusion.Sockets.NetAddress remoteAddress, Fusion.Sockets.NetConnectFailedReason reason) { }
	public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
	public void OnCustomAuthenticationResponse(NetworkRunner runner, System.Collections.Generic.Dictionary<string, object> data) { }
	public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
	public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
	public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
	public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, System.ArraySegment<byte> data) { }
	public void OnSceneLoadDone(NetworkRunner runner) { }
	public void OnSceneLoadStart(NetworkRunner runner) { }
	public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList) { }
	public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
	public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
	public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
	public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
	public void OnDisconnectedFromServer(NetworkRunner runner, Fusion.Sockets.NetDisconnectReason reason) { }
	public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, Fusion.Sockets.ReliableKey key, System.ArraySegment<byte> data) { }
	public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, Fusion.Sockets.ReliableKey key, float progress) { }
	#endregion
}

public struct PlayerInput : INetworkInput
{
	public Angle MoveDirection;
	public float MoveAmount;

	public bool Grab;
	public bool Use;

	public readonly Vector3 GetMoveVector()
	{
		return new Vector3(
			Mathf.Sin((float)MoveDirection * Mathf.Deg2Rad),
			0,
			Mathf.Cos((float)MoveDirection * Mathf.Deg2Rad)
			) * Mathf.Clamp01(MoveAmount);
	}

	public readonly bool GrabPressed(in PlayerInput prevInput)
	{
		return Grab && !prevInput.Grab;
	}

	public readonly bool UsePressed(in PlayerInput prevInput)
	{
		return Use && !prevInput.Use;
	}
}