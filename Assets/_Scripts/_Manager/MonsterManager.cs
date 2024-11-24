using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static MonsterInfoChart;
// 인스턴스 직후 몬스터에 데이터 세팅하기 위해 작성한 "데이터 캐싱용 클래스"
// 몬스터 인스턴스 하기 전에 게터세터에 세팅이 안 돼서 미리 데이터를 MonsterData 클래스에 캐싱해두고, 실제 인스턴스 후 게터세터 참조해서 데이터를 반영.
[System.Serializable]
public class MonsterData
{
    public GameObject prefab;
    public string name;
    public int lv;
    public int exp;
    public int money;
    public int dropPerc;
    public List<DropItems> dropItem;
    public int atk;
    public int def;
    public int hp;
    public float agroDistance;
    public float atkDistance;
    public float atkCooldown;
    public float moveSpeed;
    public float idleTime;
    public float moveTime;
    public float battleTime;

    public MonsterData(GameObject prefab,string name, int lv,int exp,int money,int dropPerc, List<DropItems> dropItem, int atk, int def, int hp, float agroDistance, float atkDistance, float atkCooldown, float moveSpeed, float idleTime, float moveTime, float battleTime)
    {
        this.prefab = prefab;
        this.name = name;
        this.lv = lv;
        this.exp = exp;
        this.money = money;
        this.dropPerc = dropPerc;
        this.dropItem = dropItem;
        this.atk= atk;
        this.def = def;
        this.hp = hp;
        this.agroDistance = agroDistance;
        this.atkDistance = atkDistance;
        this.atkCooldown = atkCooldown;
        this.moveSpeed = moveSpeed;
        this.idleTime = idleTime;
        this.moveTime = moveTime;
        this.battleTime = battleTime;
    }
}
public class MonsterManager : NetworkBehaviour
{
    [SerializeField] private List<GameObject> monsterPrefab;
    [SerializeField] private List<MonsterData> monsterDataList = new List<MonsterData>();

    private int currentPrefabIndex = 0; // 현재 사용할 프리팹 인덱스

    // 최대 몬스터 수를 설정하고 Networked Array로 관리
    [Networked, Capacity(5)] // Capacity는 최대 몬스터 수를 설정
    [SerializeField] NetworkArray<NetworkObject> networkedMonsters => default;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Debug.Log("[몬스터 스폰 과정 순서 메모 1]");
            StartCoroutine(LoadAndSpawnMonsters());
            Debug.Log("[몬스터 스폰 과정 순서 메모 4]");
        }
    }
    private IEnumerator LoadAndSpawnMonsters()
    {
        Debug.Log("[몬스터 스폰 과정 순서 메모 2]");

        yield return StartCoroutine(AddressableManager.instance.LoadPrefabsWithLabels("Enemy"));
        Debug.Log("[몬스터 스폰 과정 순서 메모 6]");

        // 캐싱된 프리팹을 가져와서 몬스터 리스트에 추가
        foreach (MonsterInfoChart row in BackendGameData.Instance.MonsterInfoList) // 몬스터 캐싱 차트 row 
        {
            if (row.SceneName == BackendGameData.Instance.userData.LastMap.ToString()) // 현재 씬이름과 출현 몬스터의 씬 이름이 같다면
            {
                GameObject prefab = AddressableManager.instance.GetPrefab(row.LabelName, row.PrafabName);
                if (prefab != null)
                {
                    Enemy enemyAiComponent = prefab.GetComponent<EnemyAi>();
                    if (enemyAiComponent != null)
                    {
                        // 새 MonsterData 객체를 리스트에 추가
                        monsterDataList.Add(new MonsterData(prefab,row.MonsterName,row.Lv,row.Exp,row.Money,row.MonsterDropPercent,row.Dropitem, row.Atk, row.Def, row.Hp, row.AgroDistance, row.AtkDistance, row.AtkCooldown, row.MoveSpeed, row.IdleTime, row.MoveTime, row.BattleTime));
                    }
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
                MonsterData selectedMonster = monsterDataList[currentPrefabIndex];
                NetworkObject instantiatedMonster = Runner.Spawn(selectedMonster.prefab, spawnPosition, Quaternion.identity, Object.InputAuthority);

                // EnemyAi 컴포넌트에 값을 설정
                Enemy enemyAiComponent = instantiatedMonster.GetComponent<Enemy>();
                if (enemyAiComponent != null)
                {
                    enemyAiComponent.name = selectedMonster.name;                    
                    enemyAiComponent.Lv = selectedMonster.lv;                    
                    enemyAiComponent.Exp = selectedMonster.exp;                    
                    enemyAiComponent.Money = selectedMonster.money;                    
                    enemyAiComponent.MonsterDropPercent = selectedMonster.dropPerc;   
                    enemyAiComponent.Atk = selectedMonster.atk;   
                    enemyAiComponent.Def = selectedMonster.def;   
                    enemyAiComponent.NetworkedHealth = selectedMonster.hp;   
                    enemyAiComponent.MaxHealth = selectedMonster.hp;   
                    
                    //enemyAiComponent.DropItem = selectedMonster.dropItem;                    
                    enemyAiComponent.agroDistance = selectedMonster.agroDistance;
                    enemyAiComponent.atkDistance = selectedMonster.atkDistance;
                    enemyAiComponent.atkCooldown = selectedMonster.atkCooldown;
                    enemyAiComponent.moveSpeed = selectedMonster.moveSpeed;
                    enemyAiComponent.idleTime = selectedMonster.idleTime;
                    enemyAiComponent.moveTime = selectedMonster.moveTime;
                    enemyAiComponent.battleTime = selectedMonster.battleTime;
                }

                currentPrefabIndex = (currentPrefabIndex + 1) % monsterDataList.Count;
                networkedMonsters.Set(i, instantiatedMonster.GetComponent<NetworkObject>());

                instantiatedMonster.GetComponent<Entity>().InitMonsterManager(this);
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
