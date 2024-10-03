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
        // �ʵ� ���� �� ���� ����
        SpawnMonsters();
    }

    private void SpawnMonsters()
    {
        // ��: 10���� ���͸� ����
        for (int i = 0; i < 10; i++)
        {
            NetworkObject monster = GetMonsterFromPool();
            if (monster != null)
            {
                // ���� ��ġ ���� �� Ȱ��ȭ
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
            // Ǯ�� ������ ���� ����
            NetworkObject newMonster = Runner.Spawn(monsterPrefab).GetComponent<NetworkObject>();
            activeMonsters.Add(newMonster);
            return newMonster;
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // ������ ���� ��ġ ��� ����
        return new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
    }

    public void DespawnMonster(NetworkObject monster)
    {
        // ���� ���� �� Ǯ�� ��ȯ
        monster.gameObject.SetActive(false);
        monsterPool.Enqueue(monster);
    }
}
