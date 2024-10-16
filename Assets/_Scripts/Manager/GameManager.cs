using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
// DetectChanges() �Լ� ȣ��:�� �Լ��� ȣ��Ǹ�, ChangeDetector�� �� ���� ���۸� ���մϴ�.�� ����� ��Ʈ��ũ ����ȭ�� ������Ƽ���Դϴ�. ���� ���, Networked �Ӽ��� �پ��ִ� OrderList, OrdersSpawned, OrderTimer ���� �Ӽ����� �ش�˴ϴ�.�� ���۸� ���Ͽ� ����� �Ӽ��� ���ڿ� ���·� ��ȯ�մϴ�. ���� ���, OrderList�� ����Ǿ�����, change�� "OrderList"��� ���ڿ��� ��ȯ�˴ϴ�.�� ���� ������ DetectChanges() �Լ����� ��ȯ�� ����Ʈ�� ���ԵǸ�, ���� switch (change) ������ ó���˴ϴ�.


[DefaultExecutionOrder(-200)]
public class GameManager : NetworkBehaviour, IStateAuthorityChanged, IPlayerLeft
{
    public static event System.Action<byte> OnReservedPlayerVisualsChanged;
 

    // ��Ʈ��ũ���� �����Ǵ� �ֹ� ����Ʈ, ������ �ֹ��� ��, �ֹ� Ÿ�̸�
    [Networked, Capacity(6), UnitySerializeField] public NetworkLinkedList<Order> OrderList => default;
    [Networked] public int OrdersSpawned { get; set; }
    [Networked] public TickTimer OrderTimer { get; set; }

    // �ֹ� ���� ���� �� �ֹ� ��ȿ �ð��� �����ϴ� ������
    [SerializeField] private float orderInterval = 20;
    [SerializeField] private float orderIntervalFilled = 10;
    [SerializeField] private float orderLifetime = 180;

    // �ֹ� UI ��ü�� �����ϴ� ��ųʸ�
    private Dictionary<int, FoodOrderItemUI> OrderUIs { get; } = new();

    private ChangeDetector _changes;

    #region Singleton
    public static GameManager instance;

    private void Awake()
    {
        // �̱��� ������ ����Ͽ� ���� �ν��Ͻ� ����
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
        // �ν��Ͻ��� �ı��� �� �ֹ� UI�� ����
        if (instance == this)
        {
            //CleanupOrderUIs();
            instance = null;
        }
    }
    #endregion

    // TO DO ���� ����
    public override void Spawned()
    {
        // �� �̸��� �������̽��� ǥ��
        InterfaceManager.instance.roomNameText.text = Runner.SessionInfo.Name;

        // ������ �ִ� ��� �ֹ� Ÿ�̸� ����
        //if (Object.HasStateAuthority) StartCoroutine(PrepareOrderTimer());

        // ���� ��ȭ ������ ���� �ʱ�ȭ
        //_changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        // �ֹ� ����Ʈ UI �ʱ�ȭ
        //OrdersAdded(OrderList);
    }

    //private IEnumerator PrepareOrderTimer()
    //{
        // ������ Ŭ���̾�Ʈ�� �÷��̾ ������ ������ ��� �� �ֹ� Ÿ�̸� ����
        //yield return new WaitUntil(() => Runner.TryGetPlayerObject(PlayerRef.MasterClient, out _));
        //OrderTimer = TickTimer.CreateFromSeconds(Runner, 1);
    //}

    public override void Render()
    {
        // ���� ���� üũ�� �ֹ� UI ������Ʈ
        //CheckForChanges();
        //RepaintOrders();
    }

    public override void FixedUpdateNetwork()
    {
      
        // �ֹ� Ÿ�̸Ӱ� ����Ǹ� ���ο� �ֹ� ���� �Ǵ� Ÿ�̸� �缳��
      /*  if (OrderTimer.Expired(Runner))
        {
            if (CreateOrder())
                OrderTimer = TickTimer.CreateFromSeconds(Runner, orderInterval);
            else
                OrderTimer = TickTimer.CreateFromSeconds(Runner, orderIntervalFilled);
        }*/

        // ����� �ֹ� ����
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
        //��� Ŭ���̾�Ʈ���� _changes.DetectChanges() �Լ��� ����˴ϴ�.
        // ���� ��ȭ ���� �� ���� ���׿� ���� ������Ʈ ó��
       /* foreach (string change in _changes.DetectChanges(this, out NetworkBehaviourBuffer previousBuffer, out NetworkBehaviourBuffer currentBuffer))
        {
            switch (change)
            {
                case nameof(OrderList):
                    // ������ ���� �ֹ� ����Ʈ�� ���Ͽ� ���� ���� ó��
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
        // ���� ��� ���� �÷��̾� ���־� ����ũ�� ����Ͽ� ���� �̺�Ʈ�� �߻���Ŵ
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
         // Ư�� �ε����� �÷��̾� ���־��� ��� �������� Ȯ��
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
         // ���� �ֹ� ����Ʈ�� ������� UI�� ������Ʈ
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
         // ������ ���� �ֹ� ����Ʈ�� ���Ͽ� �߰��� �ֹ��� ���ŵ� �ֹ� ó��
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
         // ���� �߰��� �ֹ��� UI�� �ݿ�
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
         // �ֹ� ����Ʈ���� �ֹ� ����
         OrderList.Remove(order);
     }

     public void CleanupOrderUIs()
     {
         // ��� �ֹ� UI ��ü ����
         foreach (var ui in OrderUIs.Values)
         {
             if (ui != null) Destroy(ui.gameObject);
         }
         OrderUIs.Clear();
     }*/

    /*public bool CreateOrder()
    {
        // �ֹ� ����Ʈ�� ���� á���� Ȯ�� �� ���ο� �ֹ� ����
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

    // ��� Ŭ�󿡼� Rpc_SubmitOrder �Լ��� ������� �޶�� �������� ��û�� �� �� ������ ������ ������ ���� Ŭ�� ��
    // ���� ���̳ĸ� ������ A�� ������ B���� 		GameManager.instance.Rpc_SubmitOrder(playerContainer); �̷� ��û�� �޴ٰ� �ϸ�
    // B�� ������ ������ ��Ʈ��ũ���� �� �Լ��� ������� �޶�� ��û�� �ϰ� ��Ʈ��ũ������ A�� �Ʒ� �Լ��� �����ϵ��� ó����
    // A���� �����ϰ� OrderList�� ����Ǹ� ��Ʈ��ũ ����ȭ�� ������ �����Ǿ� �ֱ� ������, Fusion�� �ڵ����� �� ���� ������ �ٸ� Ŭ���̾�Ʈ(B, C)���� ����ȭ��Ų��.

    /*[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_SubmitOrder(FoodContainer container)
    {
        // ����� �ֹ��� üũ�ϰ� ��Ī���� ������ ���� RPC ȣ��
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

        // ��ġ�ϴ� �ֹ��� ���� �� �ǵ��
        Rpc_OrderBad();
    }*/

    /*   [Rpc(RpcSources.StateAuthority, RpcTargets.All, InvokeLocal = true)]
       public void Rpc_OrderBad()
       {
           // �ֹ��� �߸��Ǿ��� �� UI�� ���� ȿ���� ����
           AudioManager.Play("errorUI", AudioManager.MixerTarget.SFX);
           foreach (FoodOrderItemUI ui in OrderUIs.Values) ui.FlashError();
       }*/
    #endregion

    // IsSharedModeMasterClient������ ���� Ŭ�� ������(���� ��� ������ Ŭ���̾�Ʈ�� ����Ǹ�) �ݹ����� ȣ�� �� 
    public void StateAuthorityChanged()
    {
        // ���� ��� ������ Ŭ���̾�Ʈ�� ����Ǹ� �÷��̾� �� ������Ʈ ���¸� �����ϰ� ������ �Ҵ�
        if (Runner.IsSharedModeMasterClient)
        {
            //���� ��� ������ Ŭ���̾�Ʈ�� ����Ǹ� �÷��̾� �� ������Ʈ ���¸� �����ϰ�
            CleanupLeftPlayers();
            //object.HasStateAuthority������ �ٸ� Ŭ�󿡰� �Ҵ�?����?
            AssignMasterClientAuthority();
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        // �÷��̾ ������ ���� ��� ������ Ŭ���̾�Ʈ���� �÷��̾� �� ������Ʈ ���� ����
        if (Runner.IsSharedModeMasterClient)
        {
            CleanupLeftPlayers();
            AssignMasterClientAuthority();
        }
    }

    private void AssignMasterClientAuthority()
    {
        // �濡�� ������ Ŭ���̾�Ʈ�� ������ �㿡 ���� Ŭ�󿡰� ���ο� ������ Ŭ���̾�Ʈ�� �ڵ� �Ӹ�ǰ� �Ʒ� �ڵ带 ���� ���� �Ӹ�� Ŭ�󿡰� StateAuthority ���� �ִ� �ڵ�
        if (Runner.IsSharedModeMasterClient)
        {
            // �����̴� Plane
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
        // ���� �÷��̾��� ������Ʈ�� ���� ����, �ʿ��� ��� ������ ���ġ
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
