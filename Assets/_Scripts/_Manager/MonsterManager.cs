using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class MonsterManager : NetworkBehaviour
{
    [SerializeField] private GameObject monsterPrefab;

    // 최대 몬스터 수를 설정하고 Networked Array로 관리
    [Networked, Capacity(30)] // Capacity는 최대 몬스터 수를 설정
    [SerializeField] NetworkArray<NetworkObject> networkedMonsters => default;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            SpawnMonsters();
        }
    }

    private void SpawnMonsters()
    {
        for (int i = 0; i < networkedMonsters.Length; i++)
        {
            if (networkedMonsters.Get(i) == null)
            {
                Vector3 spawnPosition = GetRandomSpawnPosition();
                NetworkObject newMonster = Runner.Spawn(monsterPrefab, spawnPosition, Quaternion.identity, Object.InputAuthority);

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
