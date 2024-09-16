using System;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisconnectUI : MonoBehaviour
{
	public static DisconnectUI Instance { get; private set; }

	public GameObject ui;
	public TMP_Text disconnectStatus;
	public TMP_Text disconnectMessage;
	public Button closeButton;

	private void Awake()
	{
		if (Instance)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		closeButton.onClick.AddListener(CloseButton);
	}

	public static void OnShutdown(ShutdownReason reason)
	{
		Debug.Log(reason);
		if (reason == ShutdownReason.Ok) return;

		(string status, string message) = ShutdownReasonToHuman(reason);
		
		Instance.disconnectStatus.text = status;
		Instance.disconnectMessage.text = message;

		Instance.ui.SetActive(true);
		Cursor.lockState = CursorLockMode.None;
	}

	public static void OnConnectFailed(NetConnectFailedReason reason)
	{
		(string status, string message) = ConnectFailedReasonToHuman(reason);

		Instance.disconnectStatus.text = status;
		Instance.disconnectMessage.text = message;

		Instance.ui.SetActive(true);
		Cursor.lockState = CursorLockMode.None;
	}

	private static (string, string) ShutdownReasonToHuman(ShutdownReason reason)
	{
		switch (reason)
		{
			case ShutdownReason.Error:
				return ("Error", "Shutdown was caused by some internal error");
			case ShutdownReason.IncompatibleConfiguration:
				return ("Incompatible Config", "Mismatching type between client Server Mode and Shared Mode");
			case ShutdownReason.ServerInRoom:
				return ("Room name in use", "There's a room with that name! Please try a different name or wait a while.");
			case ShutdownReason.DisconnectedByPluginLogic:
				return ("Disconnected By Plugin Logic", "You were kicked, the room may have been closed");
			case ShutdownReason.GameClosed:
				return ("Game Closed", "The session cannot be joined, the game is closed");
			case ShutdownReason.GameNotFound:
				return ("Game Not Found", "This room does not exist");
			case ShutdownReason.MaxCcuReached:
				return ("Max Players", "The Max CCU has been reached, please try again later");
			case ShutdownReason.InvalidRegion:
				return ("Invalid Region", "The currently selected region is invalid");
			case ShutdownReason.GameIdAlreadyExists:
				return ("ID already exists", "A room with this name has already been created");
			case ShutdownReason.GameIsFull:
				return ("Game is full", "This session is full!");
			case ShutdownReason.InvalidAuthentication:
				return ("Invalid Authentication", "The Authentication values are invalid");
			case ShutdownReason.CustomAuthenticationFailed:
				return ("Authentication Failed", "Custom authentication has failed");
			case ShutdownReason.AuthenticationTicketExpired:
				return ("Authentication Expired", "The authentication ticket has expired");
			case ShutdownReason.PhotonCloudTimeout:
				return ("Cloud Timeout", "Connection with the Photon Cloud has timed out");
			case ShutdownReason.AlreadyRunning:
				return ("Already Running", "A connection is already running");
			case ShutdownReason.InvalidArguments:
				return ("Invalid Arguments", "StartGame arguments are invalid");
			case ShutdownReason.HostMigration:
				return ("Host Migration", "The host is migrating");
			case ShutdownReason.ConnectionTimeout:
				return ("Timeout", "The remote server connection timed out");
			case ShutdownReason.ConnectionRefused:
				return ("Connection Refused", "The remote server refused the connection");
			default:
				Debug.LogWarning($"Unknown ShutdownReason {reason}");
				return ("Unknown Shutdown Reason", $"{(int)reason}");
		}
	}

	private static (string,string) ConnectFailedReasonToHuman(NetConnectFailedReason reason)
	{
		switch (reason)
		{
			case NetConnectFailedReason.Timeout:
				return ("Timed Out", "");
			case NetConnectFailedReason.ServerRefused:
				return ("Connection Refused", "The session may be currently in-game");
			case NetConnectFailedReason.ServerFull:
				return ("Server Full", "");
			default:
				Debug.LogWarning($"Unknown NetConnectFailedReason {reason}");
				return ("Unknown Connection Failure", $"{(int)reason}");
		}
	}

	private void CloseButton()
	{
		ui.SetActive(false);
		//UnityEngine.SceneManagement.SceneManager.LoadScene(0);
		UIScreen.activeScreen.BackTo(InterfaceManager.instance.MainMenuScreen);
	}
}