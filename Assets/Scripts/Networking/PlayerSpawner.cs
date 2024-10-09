using Fusion;
using System;
using System.Collections;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    public event Action<PlayerRef> OnPlayerJoined;
    public event Action<PlayerRef> OnPlayerLeft;

    [field: SerializeField] public GameObject PlayerPrefab { get; private set; }

    public void PlayerJoined(PlayerRef player)
    {
        InterfaceManager.instance.PrintPlayerCount(Runner.SessionInfo.PlayerCount, Runner.SessionInfo.MaxPlayers);

        // Start the spawn process for all players (including the local one)
        if (player == Runner.LocalPlayer)
        {
            StartCoroutine(SpawnRoutine(player));
        }
    }

    IEnumerator SpawnRoutine(PlayerRef player)
    {
        yield return new WaitUntil(() => GameManager.instance != null);
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => UIScreen.activeScreen == InterfaceManager.instance.gameplayHUD);

        if (SpawnpointManager.GetSpawnpoint(out Vector3 location, out Quaternion orientation))
        {
            Debug.Log("Spawning player");
            Runner.SpawnAsync(
                prefab: PlayerPrefab,
                position: location,
                rotation: orientation,
                inputAuthority: player,
                onCompleted: (res) =>
                {
                    if (res.IsSpawned)
                    {
                        Runner.SetPlayerObject(player, res.Object);

                        // Send the player info to the master client using a static RPC
                        RPC_NotifyMasterClient(Runner, player, res.Object.GetComponent<NetworkObject>());
                    }
                }
            );
        }
        else
        {
            Debug.LogWarning("Unable to spawn player");
        }
    }

    // Use a static RPC to notify the master client with player information
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public static void RPC_NotifyMasterClient(NetworkRunner runner, PlayerRef player, NetworkObject playerObject)
    {
        if (runner.IsSharedModeMasterClient)
        {
            Debug.Log($"Master client received player info: {player}");
            PlayerSpawner instance = FindObjectOfType<PlayerSpawner>();
            if (instance != null)
            {
                instance.HandlePlayerJoined(player);
            }
        }
    }
    public void HandlePlayerJoined(PlayerRef player)
    {
        OnPlayerJoined?.Invoke(player);
    }

    public void PlayerLeft(PlayerRef player)
    {
        InterfaceManager.instance.PrintPlayerCount(Runner.SessionInfo.PlayerCount, Runner.SessionInfo.MaxPlayers);

        if (Runner.IsSharedModeMasterClient)
        {
            Debug.Log("Master client handling player left: " + player);
            OnPlayerLeft?.Invoke(player);
            // Remove player from any tracked lists or states if necessary
        }
    }
}
