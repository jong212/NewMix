using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class MonsterManager : NetworkBehaviour
{
    [SerializeField] private GameObject monsterPrefab;
    private List<NetworkObject> activeMonsters = new List<NetworkObject>();
    private Queue<NetworkObject> monsterPool = new Queue<NetworkObject>();

    public override void Spawned()
    {
        // 필드 입장 시 몬스터 스폰
        SpawnMonsters();
    }

    private void SpawnMonsters()
    {
        // 예: 10개의 몬스터를 스폰
        for (int i = 0; i < 10; i++)
        {
            NetworkObject monster = GetMonsterFromPool();
            if (monster != null)
            {
                // 몬스터 위치 설정 및 활성화
                monster.transform.position = GetRandomSpawnPosition();
                monster.gameObject.SetActive(true);
            }
        }
    }

    private NetworkObject GetMonsterFromPool()
    {
        if (monsterPool.Count > 0)
        {
            return monsterPool.Dequeue();
        }
        else
        {
            // 풀에 없으면 새로 생성
            NetworkObject newMonster = Runner.Spawn(monsterPrefab).GetComponent<NetworkObject>();
            activeMonsters.Add(newMonster);
            return newMonster;
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // 임의의 스폰 위치 계산 로직
        return new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
    }

    public void DespawnMonster(NetworkObject monster)
    {
        // 몬스터 제거 및 풀에 반환
        monster.gameObject.SetActive(false);
        monsterPool.Enqueue(monster);
    }
}
