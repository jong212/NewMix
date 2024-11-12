using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class MonsterManager : NetworkBehaviour
{
    [SerializeField] private List<GameObject> monsterPrefab;
    private int currentPrefabIndex = 0; // 현재 사용할 프리팹 인덱스

    // 최대 몬스터 수를 설정하고 Networked Array로 관리
    [Networked, Capacity(2)] // Capacity는 최대 몬스터 수를 설정
    [SerializeField] NetworkArray<NetworkObject> networkedMonsters => default;

    public override void Spawned()
    {

        if (Object.HasStateAuthority)
        {
            Debug.Log("1");
            StartCoroutine(LoadAndSpawnMonsters());
            Debug.Log("4");

        }
    }
    private IEnumerator LoadAndSpawnMonsters()
    {
        Debug.Log("2");

        yield return StartCoroutine(AddressableManager.instance.LoadPrefabsWithLabels("Enemy"));
        Debug.Log("6");

        // 캐싱된 프리팹을 가져와서 몬스터 리스트에 추가
        foreach (MonsterInfoChart row in BackendGameData.Instance.MonsterInfoList)
        {
            if (row.SceneName == BackendGameData.Instance.userData.LastMap.ToString())
            {
                GameObject prefab = AddressableManager.instance.GetPrefab(row.LabelName, row.PrafabName);
                if (prefab != null)
                {
                    monsterPrefab.Add(prefab);
                    Debug.Log($"[로드 후 캐싱 완료]: {row.PrafabName}");
                }
                else
                {
                    Debug.LogError($"Failed to load prefab: {row.PrafabName}");
                }
            }
        }

        if (monsterPrefab.Count == 0)
        {
            Debug.LogError("No prefabs loaded for spawning.");
            yield break;
        }

        Debug.Log("[4-3] 적 모델 로드 완료");
        SpawnMonsters();
    }
    private void SpawnMonsters()
    {
        for (int i = 0; i < networkedMonsters.Length; i++)
        {
            if (networkedMonsters.Get(i) == null)
            {
                Vector3 spawnPosition = GetRandomSpawnPosition();
                // 번갈아 가면서 프리팹 선택
                GameObject selectedPrefab = monsterPrefab[currentPrefabIndex];
                selectedPrefab.transform.position = spawnPosition;
                currentPrefabIndex = (currentPrefabIndex + 1) % monsterPrefab.Count; // 인덱스를 순환시킴
                NetworkObject newMonster = Runner.Spawn(selectedPrefab, spawnPosition, Quaternion.identity, Object.InputAuthority);

                // 네트워크ed 배열에 추가
                networkedMonsters.Set(i, newMonster);

                // 몬스터 초기화
                newMonster.GetComponent<Entity>().InitMonsterManager(this);
            }
        }
    }

    public void DespawnMonster(NetworkObject monster)
    {
        if (Object.HasStateAuthority)
        {
            monster.gameObject.SetActive(false);

            // 5초 후에 몬스터 재스폰 코루틴 실행
            StartCoroutine(RespawnMonsterAfterDelay(monster, 5f));
        }
    }

    private IEnumerator RespawnMonsterAfterDelay(NetworkObject monster, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (Object.HasStateAuthority)
        {
            monster.transform.position = GetRandomSpawnPosition();
            monster.gameObject.SetActive(true);
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        return new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
    }

}
