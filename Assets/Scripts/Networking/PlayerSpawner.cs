using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    public event Action<PlayerRef> OnPlayerJoined;
    public event Action<PlayerRef> OnPlayerLeft;
    [field: SerializeField] public GameObject PlayerPrefab { get; private set; }

    public void PlayerJoined(PlayerRef player)
	{
		InterfaceManager.instance.PrintPlayerCount(Runner.SessionInfo.PlayerCount, Runner.SessionInfo.MaxPlayers);

        if (player == Runner.LocalPlayer)
		{
			StartCoroutine(SpawnRoutine());

        }
        else
		{
            OnPlayerJoined?.Invoke(player);

            InterfaceManager.instance.ChefIconShake();
		}

		IEnumerator SpawnRoutine()
		{
			//yield return new WaitUntil(() => SpawnpointManager.Instance != null);
			yield return new WaitUntil(() => GameManager.instance != null);

			//UIScreen.Focus(InterfaceManager.instance.playerSettingScreen);

			yield return new WaitForEndOfFrame();

			//GameManager.instance.ReservedPlayerVisualsChanged();

			yield return new WaitUntil(() => UIScreen.activeScreen == InterfaceManager.instance.gameplayHUD);

			if (SpawnpointManager.GetSpawnpoint(out Vector3 location, out Quaternion orientation))
			{
				Debug.Log("Spawning player");
				Runner.SpawnAsync(
					prefab: PlayerPrefab,
					position: location,
					rotation: orientation,
					inputAuthority: player,
					onCompleted: (res) => { if (res.IsSpawned) Runner.SetPlayerObject(Runner.LocalPlayer, res.Object);
                        OnPlayerJoined?.Invoke(Runner.LocalPlayer); // 스폰 완료 후에 이벤트 호출

                    }
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
		//GameManager.instance.ReservedPlayerVisualsChanged();
		if (Runner.IsSharedModeMasterClient)
		{
			Debug.Log("LeftTest : " + "1");
            OnPlayerLeft?.Invoke(player);
        }
    }
}