using Fusion;
using System;
using System.Collections;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    public event Action<PlayerRef> OnPlayerJoined;
    public event Action<PlayerRef> OnPlayerLeft;
    private GameObject playerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
         
        if (player == Runner.LocalPlayer)
        {
            StartCoroutine(SpawnRoutine(player));
        }
    }

    IEnumerator SpawnRoutine(PlayerRef player)
    {
        yield return new WaitUntil(() => GameManager.instance != null);
        yield return new WaitForEndOfFrame();
       
        (string labelName, string prefabName) = StaticManager.DataSetManager.CharacterDefaultSettings(); 
        if(string.IsNullOrEmpty(labelName) || string.IsNullOrEmpty(prefabName)){
            Debug.LogWarning("[플레이어 스포너에서 스폰할 때 플레이어의 캐릭터 어드레서블 라벨 혹은 프리팹 이름 값을 불러오지 못함]");
            yield break;
        } else
        {
           Debug.Log($"[5 PlayerSpawner : 플레이어 에게 적용할 어드레서블 레이블,프리팹이름 값 정상적으로 가져옴 {labelName}, {prefabName} ]");
        }
        // 로드 완료 여부를 추적하는 변수들
        bool isInventoryLoaded = false;
        bool isPlayerPrefabLoaded = false;
        bool isEnemyPrefabLoad = false;
        bool isSpriteLoaded = false;
        bool isLoaded = false;

        // 어드레서블 로드 시작
        AddressableManager.instance.LoadPrefabsWithLabel("Inventory", () =>
        {
            isInventoryLoaded = true;
            CheckIfAllLoaded();
            //TEMPHIDE// Debug.Log("InventoryItem loaded!");
        });
        AddressableManager.instance.LoadPrefabsWithLabel("Enemy", () =>
        {
            isEnemyPrefabLoad = true;
            CheckIfAllLoaded();
            //TEMPHIDE//  Debug.Log("Enemy loaded!");
        });
        AddressableManager.instance.LoadPrefabsWithLabel(labelName, () =>
        {
            
            playerPrefab = AddressableManager.instance.GetPrefab(labelName, prefabName);
            isPlayerPrefabLoaded = true;
            CheckIfAllLoaded();
            //TEMPHIDE// Debug.Log("PlayerPrefab loaded!"); 
        });        
        AddressableManager.instance.LoadSpritesWithLabel("Sprite", () =>
        {
            isSpriteLoaded = true;
            CheckIfAllLoaded();
            //TEMPHIDE// Debug.Log("Sprite loaded!"); 
        });

        // 내부 함수: 두 로드 완료 여부를 확인
        void CheckIfAllLoaded()
        {
            if (isInventoryLoaded && isPlayerPrefabLoaded && isEnemyPrefabLoad && isSpriteLoaded)
            {
                isLoaded = true;
            }
        }        
        
        // 프리팹 로드가 완료될 때까지 대기
        yield return new WaitUntil(() => isLoaded);
        StaticManager.Instance.AllLoad = true;
        if (SpawnpointManager.GetSpawnpoint(out Vector3 location, out Quaternion orientation))
        {
            //TEMPHIDE// Debug.Log("Spawning player");
            Runner.SpawnAsync(
                prefab: playerPrefab,
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
        if (Runner.IsSharedModeMasterClient)
        {
            Debug.Log("Master client handling player left: " + player);
            OnPlayerLeft?.Invoke(player);
            // Remove player from any tracked lists or states if necessary
        }
    }
}
