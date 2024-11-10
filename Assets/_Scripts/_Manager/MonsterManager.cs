using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class MonsterManager : NetworkBehaviour
{
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] List<NetworkObject> activeMonsters = new List<NetworkObject>();
    [SerializeField] Queue<NetworkObject> monsterPool = new Queue<NetworkObject>();
    public override void Spawned()
    {
        Debug.Log("test11111");
        if (Object.HasStateAuthority)
        {
            Debug.Log("test22222");

            SpawnMonsters();
        }
    }
    private void Update()
    {
    }
    private void SpawnMonsters()
    {
        for (int i = 0; i < 1; i++) // 조건에 따라 스폰할 몬스터 수 조정 가능
        {
            NetworkObject monster = GetMonsterFromPool();
            if (monster != null)
            {
                // 몬스터 위치 설정 및 활성화
                monster.transform.position = GetRandomSpawnPosition();
                monster.gameObject.SetActive(true);

                // 활성 몬스터 리스트에 추가
                activeMonsters.Add(monster);
                monster.GetComponent<Entity>().InitMonsterManager(this);
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
            NetworkObject newMonster = Runner.Spawn(monsterPrefab);
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
        if (Object.HasStateAuthority)
        {
            // 몬스터 비활성화
            monster.gameObject.SetActive(false);

            // 활성 몬스터 리스트에서 제거
            activeMonsters.Remove(monster);

            // 몬스터를 풀에 반환
            monsterPool.Enqueue(monster);

            // 5초 후에 몬스터 재스폰 코루틴 실행
            Runner.StartCoroutine(RespawnMonsterAfterDelay(monster, 5f));
        }
    }

    private IEnumerator<WaitForSeconds> RespawnMonsterAfterDelay(NetworkObject monster, float delay)
    {
        // 5초 대기
        yield return new WaitForSeconds(delay);

        if (Object.HasStateAuthority)
        {
            // 몬스터 재스폰
            SpawnMonsterFromPool(monster);
        }
    }

    private void SpawnMonsterFromPool(NetworkObject monster)
    {
        if (monsterPool.Contains(monster))
        {
            // 풀에서 제거
            monsterPool = new Queue<NetworkObject>(monsterPool);
            monsterPool.Dequeue();

            // 몬스터 위치 설정 및 활성화
            monster.transform.position = GetRandomSpawnPosition();
            monster.gameObject.SetActive(true);

            // 활성 몬스터 리스트에 추가
            activeMonsters.Add(monster);
            monster.GetComponent<Entity>().InitMonsterManager(this);

        }
    }
}
