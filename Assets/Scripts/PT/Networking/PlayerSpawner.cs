using Fusion;
using System.Collections;
using UnityEngine;

// 이 스크립트는 Prtorype runner 프리팹에 연결되어 있고 
public class PlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
	[field: SerializeField] public GameObject PlayerPrefab { get; private set; }

    //Fusion에서 IPlayerJoined 인터페이스를 구현한 클래스는, 해당 인터페이스의 PlayerJoined 메서드를 통해 플레이어가 세션에 참여할 때 자동으로 호출됩니다. 이 메커니즘은 다음과 같이 작동합니다:

    public void PlayerJoined(PlayerRef player)
	{
		InterfaceManager.instance.PrintPlayerCount(Runner.SessionInfo.PlayerCount, Runner.SessionInfo.MaxPlayers);

		if (player == Runner.LocalPlayer)
		{
			StartCoroutine(SpawnRoutine());
		}
		else
		{
			InterfaceManager.instance.ChefIconShake();
		}

		IEnumerator SpawnRoutine()
		{
			yield return new WaitUntil(() => SpawnpointManager.Instance != null);
			yield return new WaitUntil(() => GameManager.instance != null);

			// 세션 생성 성공 시 캐릭터 선택 UI팝업 열리도록 하는 로직
			UIScreen.Focus(InterfaceManager.instance.playerSettingScreen);

			yield return new WaitForEndOfFrame();

			// force color availability to be evaluated
			GameManager.instance.ReservedPlayerVisualsChanged();

			// Wait until the client has selected their nickname/visual before giving them an avatar
			yield return new WaitUntil(() => UIScreen.activeScreen == InterfaceManager.instance.gameplayHUD);

			if (SpawnpointManager.GetSpawnpoint(out Vector3 location, out Quaternion orientation))
			{
				Debug.Log("Spawning player");
				Runner.SpawnAsync(
					prefab: PlayerPrefab,
					position: location,
					rotation: orientation,
					inputAuthority: player,
					onCompleted: (res) => { if (res.IsSpawned) Runner.SetPlayerObject(Runner.LocalPlayer, res.Object); }
				);
			}
			else
			{
				Debug.LogWarning("Unable to spawn player");
			}
		}
	}

	public void PlayerLeft(PlayerRef player)
	{
		InterfaceManager.instance.PrintPlayerCount(Runner.SessionInfo.PlayerCount, Runner.SessionInfo.MaxPlayers);
		GameManager.instance.ReservedPlayerVisualsChanged();
	}
}