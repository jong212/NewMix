using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
// DetectChanges() 함수 호출:이 함수가 호출되면, ChangeDetector는 두 개의 버퍼를 비교합니다.비교 대상은 네트워크 동기화된 프로퍼티들입니다. 예를 들어, Networked 속성이 붙어있는 OrderList, OrdersSpawned, OrderTimer 등의 속성들이 해당됩니다.두 버퍼를 비교하여 변경된 속성을 문자열 형태로 반환합니다. 예를 들어, OrderList가 변경되었으면, change로 "OrderList"라는 문자열이 반환됩니다.이 변경 사항은 DetectChanges() 함수에서 반환된 리스트에 포함되며, 이후 switch (change) 문에서 처리됩니다.


[DefaultExecutionOrder(-200)]
public class GameManager : NetworkBehaviour, IStateAuthorityChanged, IPlayerLeft
{

    #region Singleton
    public static GameManager instance;

    private void Awake()
    {
        // 싱글톤 패턴을 사용하여 단일 인스턴스 유지
        if (instance)
        {
             Debug.LogWarning("Instance already exists!");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void OnDestroy()
    {
        // 인스턴스가 파괴될 때 주문 UI를 정리
        if (instance == this)
        {
            //CleanupOrderUIs();
            instance = null;
        }
    }
    #endregion

    // TO DO 몬스터 스폰
    public override void Spawned()
    {
         
        if (StaticManager.UI.MonsterInventoryManagerUI.SceneChangeInit == true)
        {

        }
    }

    public override void Render()
    {
        
    }

    public override void FixedUpdateNetwork()
    {
    }
    public void SpawnMonsterData()
    {
        List<SetMymon> setMon = BackendGameData.Instance.userData.setMymonList;
        List<MonsterInfoChart> itemChart = BackendGameData.Instance.MonsterInfoList;

        foreach (SetMymon setInvenIdx in setMon)
        {
                foreach (MonsterInfoChart item in itemChart)
                {
                    if (setInvenIdx.setMonList.Count > 0 && setInvenIdx.setMonList[0] == (int)item.MonsterId)
                    {
                        Sprite spriteImg = AddressableManager.instance.GetSprite(item.MyMonSpriteName);
                        if (spriteImg != null)
                        {
                            InsertMyMonsters(setInvenIdx, item);
                        }
                        break;
                    }
                }
        }
    }
    public void InsertMyMonsters(SetMymon myMonsterStat, MonsterInfoChart monsterChart)
    {
        

        GameObject playerPrefab = AddressableManager.instance.GetPrefab("MyMonster", monsterChart.PrafabName);
        Transform trs = StaticManager.Instance.UniquePlayer.transform;
        Runner.SpawnAsync(
            prefab: playerPrefab,
            position: trs.position,
            rotation: default,
            inputAuthority: default,
            onCompleted: (res) =>
            {
                if (res.IsSpawned)
                {
                    MycoreNetwork Network = res.Object.GetComponent<MycoreNetwork>();
                    Network.Lv = myMonsterStat.setMonList[1];
                    Network.Atk = myMonsterStat.setMonList[2];
                    Network.Def = myMonsterStat.setMonList[3];
                    Network.Hp = myMonsterStat.setMonList[4];
                    // Send the player info to the master client using a static RPC
                }
            }
        );
    }
    // IsSharedModeMasterClient권한을 가진 클라가 나가면(공유 모드 마스터 클라이언트가 변경되면) 콜백으로 호출 됨 
    public void StateAuthorityChanged()
    {
        // 공유 모드 마스터 클라이언트가 변경되면 플레이어 및 오브젝트 상태를 정리하고 권한을 할당
        if (Runner.IsSharedModeMasterClient)
        {
            //공유 모드 마스터 클라이언트가 변경되면 플레이어 및 오브젝트 상태를 정리하고
            CleanupLeftPlayers();
            //object.HasStateAuthority권한을 다른 클라에게 할당?이전?
            AssignMasterClientAuthority();
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        // 플레이어가 나가면 공유 모드 마스터 클라이언트에서 플레이어 및 오브젝트 상태 정리
        if (Runner.IsSharedModeMasterClient)
        {
            CleanupLeftPlayers();
            AssignMasterClientAuthority();
        }
    }

    private void AssignMasterClientAuthority()
    {
        // 방에서 마스터 클라이언트가 나가면 담에 남은 클라에게 새로운 마스터 클라이언트로 자동 임명되고 아래 코드를 통해 새로 임명된 클라에게 StateAuthority 권한 주는 코드
        if (Runner.IsSharedModeMasterClient)
        {
            // 움직이는 Plane
            var platforms = FindObjectsOfType<NetworkMovingPlatform>()
                .Where(p => !p.Object.HasStateAuthority);
            foreach (var platform in platforms)
            {
                platform.Object.RequestStateAuthority();
            }

            var Enemys = FindObjectsOfType<EnemyAi>()
                .Where(e => !e.Object.HasStateAuthority);
            foreach (var platform in Enemys)
            {
                platform.Object.RequestStateAuthority();
            }
        }
    }

    void CleanupLeftPlayers()
    {
        // 나간 플레이어의 오브젝트와 상태 정리, 필요한 경우 아이템 재배치
        Character[] objs = FindObjectsOfType<Character>()
            .Where(c => !Runner.ActivePlayers.Contains(c.Object.StateAuthority))
            .ToArray();

        foreach (Character c in objs)
        {
            if (c.Object.IsValid)
            {
                c.GetComponent<AuthorityHandler>().RequestAuthority(() =>
                {
                    Item item = c.HeldItem;
                    if (item)
                    {
                        WorkSurface surf = FindObjectsOfType<WorkSurface>()
                            .OrderBy(w => Vector2.Distance(
                                new Vector2(c.transform.position.x, c.transform.position.z),
                                new Vector2(w.transform.position.x, w.transform.position.z)))
                            .FirstOrDefault(w => w.ItemOnTop == null);

                        if (surf)
                        {
                            surf.GetComponent<AuthorityHandler>().RequestAuthority(() =>
                            {
                                surf.ItemOnTop = item;
                                item.transform.SetPositionAndRotation(surf.SurfacePoint.position, surf.SurfacePoint.rotation);
                                item.transform.SetParent(surf.Object.transform, true);
                                Runner.Despawn(c.Object);
                            });
                        }
                        else
                        {
                            Runner.Despawn(item.Object);
                            Runner.Despawn(c.Object);
                        }
                    }
                    else Runner.Despawn(c.Object);
                });
            }
        }
    }
}
