using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
// DetectChanges() 함수 호출:이 함수가 호출되면, ChangeDetector는 두 개의 버퍼를 비교합니다.비교 대상은 네트워크 동기화된 프로퍼티들입니다. 예를 들어, Networked 속성이 붙어있는 OrderList, OrdersSpawned, OrderTimer 등의 속성들이 해당됩니다.두 버퍼를 비교하여 변경된 속성을 문자열 형태로 반환합니다. 예를 들어, OrderList가 변경되었으면, change로 "OrderList"라는 문자열이 반환됩니다.이 변경 사항은 DetectChanges() 함수에서 반환된 리스트에 포함되며, 이후 switch (change) 문에서 처리됩니다.


[DefaultExecutionOrder(-200)]
public class GameManager : NetworkBehaviour, IStateAuthorityChanged, IPlayerLeft
{
    public static event System.Action<byte> OnReservedPlayerVisualsChanged;
 

    // 네트워크에서 관리되는 주문 리스트, 생성된 주문의 수, 주문 타이머
    [Networked, Capacity(6), UnitySerializeField] public NetworkLinkedList<Order> OrderList => default;
    [Networked] public int OrdersSpawned { get; set; }
    [Networked] public TickTimer OrderTimer { get; set; }

    // 주문 생성 간격 및 주문 유효 시간을 설정하는 변수들
    [SerializeField] private float orderInterval = 20;
    [SerializeField] private float orderIntervalFilled = 10;
    [SerializeField] private float orderLifetime = 180;

    // 주문 UI 객체를 관리하는 딕셔너리
    private Dictionary<int, FoodOrderItemUI> OrderUIs { get; } = new();

    private ChangeDetector _changes;

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
        // 방 이름을 인터페이스에 표시
        InterfaceManager.instance.roomNameText.text = Runner.SessionInfo.Name;

        // 권한이 있는 경우 주문 타이머 설정
        //if (Object.HasStateAuthority) StartCoroutine(PrepareOrderTimer());

        // 상태 변화 감지를 위한 초기화
        //_changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        // 주문 리스트 UI 초기화
        //OrdersAdded(OrderList);
    }

    //private IEnumerator PrepareOrderTimer()
    //{
        // 마스터 클라이언트가 플레이어를 스폰할 때까지 대기 후 주문 타이머 시작
        //yield return new WaitUntil(() => Runner.TryGetPlayerObject(PlayerRef.MasterClient, out _));
        //OrderTimer = TickTimer.CreateFromSeconds(Runner, 1);
    //}

    public override void Render()
    {
        // 상태 변경 체크와 주문 UI 업데이트
        //CheckForChanges();
        //RepaintOrders();
    }

    public override void FixedUpdateNetwork()
    {
      
        // 주문 타이머가 만료되면 새로운 주문 생성 또는 타이머 재설정
      /*  if (OrderTimer.Expired(Runner))
        {
            if (CreateOrder())
                OrderTimer = TickTimer.CreateFromSeconds(Runner, orderInterval);
            else
                OrderTimer = TickTimer.CreateFromSeconds(Runner, orderIntervalFilled);
        }*/

        // 만료된 주문 제거
        /*foreach (Order order in OrderList)
        {
            if (order.IsValid)
            {
                float time = ((int)Runner.Tick - order.TickCreated) * Runner.DeltaTime / orderLifetime;
                if (time >= 1)
                {
                    RemoveOrder(order);
                    break;
                }
            }
        }*/
    }

    private void CheckForChanges()
    {
        //모든 클라이언트에서 _changes.DetectChanges() 함수가 실행됩니다.
        // 상태 변화 감지 및 변경 사항에 따른 업데이트 처리
       /* foreach (string change in _changes.DetectChanges(this, out NetworkBehaviourBuffer previousBuffer, out NetworkBehaviourBuffer currentBuffer))
        {
            switch (change)
            {
                case nameof(OrderList):
                    // 이전과 현재 주문 리스트를 비교하여 변경 사항 처리
                    var reader = GetLinkListReader<Order>(nameof(OrderList));
                    var previous = reader.Read(previousBuffer);
                    var current = reader.Read(currentBuffer);
                    OrdersChanged(previous, current);
                    break;
            }
        }*/
    }

    #region Player Visuals
    /*public void ReservedPlayerVisualsChanged()
    {
        // 현재 사용 중인 플레이어 비주얼 마스크를 계산하여 변경 이벤트를 발생시킴
        byte mask = (byte)Runner.ActivePlayers
            .Select(p => Runner.TryGetPlayerObject(p, out NetworkObject pObj) ? pObj : null)
            .Where(p => p != null)
            .Select(o => o.GetBehaviour<Character>())
            .Select(c => 1 << c.Visual)
            .Sum();

        Debug.Log("Visuals reserved mask: " + System.Convert.ToString(mask, 2).PadLeft(8, '0'));

        OnReservedPlayerVisualsChanged?.Invoke(mask);
    }*/

    /* public bool IsPlayerVisualAvailable(int index)
     {
         // 특정 인덱스의 플레이어 비주얼이 사용 가능한지 확인
         return Runner.ActivePlayers
             .Select(p => Runner.TryGetPlayerObject(p, out NetworkObject pObj) ? pObj : null)
             .Where(p => p != null)
             .Select(o => o.GetBehaviour<Character>())
             .Any(c => c.Visual == index) == false;
     }*/
    #endregion

    #region Orders
    /* private void RepaintOrders()
     {
         // 현재 주문 리스트를 기반으로 UI를 업데이트
         int t = Runner.Tick;
         foreach (Order order in OrderList)
         {
             if (order.IsValid)
             {
                 float time = (t - order.TickCreated) * Runner.DeltaTime / orderLifetime;
                 if (OrderUIs.TryGetValue(order.Id, out var ui))
                 {
                     ui.Paint(1 - time);
                 }
             }
         }
     }

     private void OrdersChanged(NetworkLinkedListReadOnly<Order> prevOrders, NetworkLinkedListReadOnly<Order> currOrders)
     {
         // 이전과 현재 주문 리스트를 비교하여 추가된 주문과 제거된 주문 처리
         IEnumerable<Order> prevEnum = prevOrders.Enumerable();
         IEnumerable<Order> currEnum = currOrders.Enumerable();

         IEnumerable<Order> added = currEnum.Except(prevEnum);
         IEnumerable<Order> removed = prevEnum.Except(currEnum);

         foreach (var rem in removed)
         {
             float time = ((int)Runner.Tick - rem.TickCreated) * Runner.DeltaTime / orderLifetime;
             if (time < 1)
             {
                 AudioManager.Play("successUI", AudioManager.MixerTarget.SFX);
                 OrderUIs[rem.Id].FlashComplete();
             }
             OrderUIs[rem.Id].Expire();
             OrderUIs.Remove(rem.Id);
         }

         OrdersAdded(added);
     }

     private void OrdersAdded(IEnumerable<Order> added)
     {
         // 새로 추가된 주문을 UI에 반영
         foreach (var add in added)
         {
             var ingredients =
                 add.IngredientKeys
                 .Select(k => ResourcesManager.instance.ingredientBank.GetValue<IngredientData>(k))
                 .Where(v => v != null)
                 .ToArray();

             if (ingredients.Length > 0)
             {
                 FoodOrderItemUI orderUI = Instantiate(
                     ResourcesManager.instance.foodOrderUIPrefab,
                     InterfaceManager.instance.foodOrderItemHolder);
                 orderUI.SetOrder(ingredients);
                 orderUI.orderNumber.text = $"{add.Id}";

                 OrderUIs.Add(add.Id, orderUI);
             }
         }
     }

     private void RemoveOrder(Order order)
     {
         // 주문 리스트에서 주문 제거
         OrderList.Remove(order);
     }

     public void CleanupOrderUIs()
     {
         // 모든 주문 UI 객체 정리
         foreach (var ui in OrderUIs.Values)
         {
             if (ui != null) Destroy(ui.gameObject);
         }
         OrderUIs.Clear();
     }*/

    /*public bool CreateOrder()
    {
        // 주문 리스트가 가득 찼는지 확인 후 새로운 주문 생성
        if (OrderList.Count >= OrderList.Capacity) return false;

        OrdersSpawned++;

        IngredientData[] orderIngredients = OrderMenu.GetOrder();
        short[] ingredientKeys = orderIngredients.Select(i => ResourcesManager.instance.ingredientBank.GetKey(i)).ToArray();

        Order order = new()
        {
            TickCreated = Runner.Tick,
            Id = OrdersSpawned
        };
        order.IngredientKeys.CopyFrom(ingredientKeys, 0, ingredientKeys.Length);

        OrderList.Add(order);
        return true;
    }*/

    // 모든 클라에서 Rpc_SubmitOrder 함수를 실행시켜 달라고 서버한테 요청은 할 수 있지만 실행은 권한을 가진 클라가 함
    // 무슨 말이냐면 권한을 A가 가지고 B에서 		GameManager.instance.Rpc_SubmitOrder(playerContainer); 이런 요청을 햇다고 하면
    // B는 권한이 없으니 네트워크에게 이 함수를 실행시켜 달라고 요청만 하고 네트워크에서는 A가 아래 함수를 실행하도록 처리함
    // A에서 실행하고 OrderList가 변경되면 네트워크 동기화된 변수로 설정되어 있기 때문에, Fusion은 자동으로 이 변경 사항을 다른 클라이언트(B, C)에게 동기화시킨다.

    /*[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_SubmitOrder(FoodContainer container)
    {
        // 제출된 주문을 체크하고 매칭되지 않으면 오류 RPC 호출
        HashSet<IngredientData> containerIngredientData = container.Ingredients.Select(i => i.Data).ToHashSet();

        container.GetBehaviour<AuthorityHandler>().Rpc_Despawn();

        foreach (Order order in OrderList)
        {
            IngredientData[] orderIngredients = order.IngredientKeys
                .Where(i => i != 0)
                .Select(i => ResourcesManager.instance.ingredientBank.GetValue<IngredientData>(i))
                .ToArray();

            if (containerIngredientData.SetEquals(orderIngredients))
            {
                RemoveOrder(order);
                return;
            }
        }

        // 일치하는 주문이 없을 때 피드백
        Rpc_OrderBad();
    }*/

    /*   [Rpc(RpcSources.StateAuthority, RpcTargets.All, InvokeLocal = true)]
       public void Rpc_OrderBad()
       {
           // 주문이 잘못되었을 때 UI와 사운드 효과를 적용
           AudioManager.Play("errorUI", AudioManager.MixerTarget.SFX);
           foreach (FoodOrderItemUI ui in OrderUIs.Values) ui.FlashError();
       }*/
    #endregion

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

            /*var Enemys = FindObjectsOfType<EnemyNetwork>()
                .Where(e => !e.Object.HasStateAuthority);
            foreach (var platform in Enemys)
            {
                platform.Object.RequestStateAuthority();
            }*/
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
