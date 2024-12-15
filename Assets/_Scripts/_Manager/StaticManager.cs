using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StaticManager : MonoBehaviour
{
    public static StaticManager Instance { get; private set; }      // 싱글톤
    public static UIManager UI { get; private set; }                // 인스펙터 참조하기 위해 public
    public WorldCanvas WorldCanvas { get; set; }
    public static DataSetManager DataSetManager { get; private set; }                // 인스펙터 참조하기 위해 public 
    private Queue<Action> InventoryQueue = new Queue<Action>();
    private bool isProcessing = false;  
    public bool AllLoad { get; set; }
    public UserData CashUdata { get;  set; }
    public event Action Stat;
    [SerializeField] private Character _uniquePlayer;
    public Character UniquePlayer {
        get => _uniquePlayer;
        set => _uniquePlayer = value;
    }
    void Awake()
    {
        Init();
    }
    void Init()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        UI = GetComponentInChildren<UIManager>();
        UI.Init();
        DataSetManager = GetComponentInChildren<DataSetManager>();
        WorldCanvas = GetComponentInChildren<WorldCanvas>();
        /*DataSetManager = GetComponentInChildren<DataSetManager>(); 필요할 때 사용 아직 스태틱 매니저에서는 뭐 처리할 게 없어 보임*/
    }
    public void EnqueueAction(Action action)
    {
        InventoryQueue.Enqueue(action);
        if (!isProcessing)
        {
            StartCoroutine(ProcessActions());
        }
    }
    private IEnumerator ProcessActions()
    {
        isProcessing = true;
        while (InventoryQueue.Count > 0)
        {
            Action currentAction = InventoryQueue.Dequeue();
            currentAction.Invoke(); // 작업 실행
            Stat?.Invoke(); 
            yield return null; // 다음 프레임까지 대기
        }
        isProcessing = false;
    }

    // 모바일에서 오브젝트 위치 확인용
    public void LogHierarchyPath(Transform transform)
    {
        if (transform == null)
        {
            return;
        }

        string path = transform.name;
        Transform currentParent = transform.parent;

        // 부모를 따라 올라가며 전체 경로 생성
        while (currentParent != null)
        {
            path = currentParent.name + "/" + path;
            currentParent = currentParent.parent;
        }
    }
   
    public void CashData()
    {
        UserData cashPlayerData = BackendGameData.Instance.userData;
    }

    public void ConfirmleaveSessionHook()
    {
        StartCoroutine(LeaveAndJoinNewSession());
    }
    private IEnumerator LeaveAndJoinNewSession()
    {
        UI.Loading.gameObject.SetActive(true);

        Matchmaker.Instance.Runner.Shutdown();

        // 잠시 대기하여 Runner가 완전히 정리될 시간을 준다
        yield return new WaitForSeconds(1);
        Matchmaker.Instance.TryConnectShared();
    }
     public void InvenItemSwap(int changeA, int ChangeB)
    {
        EnqueueAction(() =>
        {
            InvenItemSwapQueue(changeA, ChangeB);
        });
    }
    private void InvenItemSwapQueue(int beforeSloatId, int afterSloatId)
    {
        var copyBeforeItemId   = CashUdata.InventorySlots[beforeSloatId].ItemId;
        var copyBeforeQuantity = CashUdata.InventorySlots[beforeSloatId].Quantity;

        CashUdata.InventorySlots[beforeSloatId].ItemId = CashUdata.InventorySlots[afterSloatId].ItemId;
        CashUdata.InventorySlots[beforeSloatId].Quantity= CashUdata.InventorySlots[afterSloatId].Quantity;

        CashUdata.InventorySlots[afterSloatId].ItemId = copyBeforeItemId;
        CashUdata.InventorySlots[afterSloatId].Quantity = copyBeforeQuantity;

        string inventoryJson = JsonMapper.ToJson(new { slots = CashUdata.InventorySlots });
        BackendGameData.Instance.GameDataUpdate<string>("Inventory", inventoryJson); 
    }
    public void InvenItemMove(int changeA, int ChangeB)
    {
        EnqueueAction(() =>
        {
            InvenItemMoveQueue(changeA, ChangeB);
        });
    }
    private void InvenItemMoveQueue(int beforeSloatId, int afterSloatId)
    {
        var copyBeforeItemId = CashUdata.InventorySlots[beforeSloatId].ItemId;
        var copyBeforeQuantity = CashUdata.InventorySlots[beforeSloatId].Quantity;

        CashUdata.InventorySlots[beforeSloatId].ItemId = null;
        CashUdata.InventorySlots[beforeSloatId].Quantity = 0;

        CashUdata.InventorySlots[afterSloatId].ItemId = copyBeforeItemId;
        CashUdata.InventorySlots[afterSloatId].Quantity = copyBeforeQuantity;

        string inventoryJson = JsonMapper.ToJson(new { slots = CashUdata.InventorySlots });
        BackendGameData.Instance.GameDataUpdate<string>("Inventory", inventoryJson);
    }
    public void SubInvenToInven(InventoryType type, int ChangeB)
    {
        EnqueueAction(() =>
        {
            SubInvenToInvenQueue(type, ChangeB);
        });
    }
    private void SubInvenToInvenQueue(InventoryType type, int afterSloatId)
    {
        // 참조 캐싱
        var tempUserdata = BackendGameData.Instance.userData;

        int currentInventoryIdx = tempUserdata.setPlayerItems[(int)type];

        // 뒤끝 Inventory 컬럼 업데이트
        CashUdata.InventorySlots[afterSloatId].ItemId = currentInventoryIdx;
        CashUdata.InventorySlots[afterSloatId].Quantity = 1;
        string inventoryJson = JsonMapper.ToJson(new { slots = CashUdata.InventorySlots });
        BackendGameData.Instance.GameDataUpdate<string>("Inventory", inventoryJson);

        // 뒤끝 SetPlayerItems 컬럼 업데이트
        BackendGameData.Instance.userData.UpdatePlayerItemAt((int)type,0);

        // UI 무기 해제 업데이트
        UniquePlayer.InitItem();
    }
    public void DoubleClickItem(InventoryType type, int subInvenIdx)
    {
        EnqueueAction(() =>
        {
            DoubleClickItemQueue(type, subInvenIdx);
        });
    }
    void DoubleClickItemQueue(InventoryType type, int subInvenIdx)
    {
        var tempUserdata = BackendGameData.Instance.userData;
        int invenIdxValue = tempUserdata.setPlayerItems[(int)type];

        int v = CashUdata.InventorySlots[subInvenIdx].ItemId.Value;
        
        CashUdata.InventorySlots[subInvenIdx].ItemId = invenIdxValue;
        CashUdata.InventorySlots[subInvenIdx].Quantity = (invenIdxValue != 0) ? 1 : 0;
        string inventoryJson = JsonMapper.ToJson(new { slots = CashUdata.InventorySlots });
        BackendGameData.Instance.GameDataUpdate<string>("Inventory", inventoryJson);

        BackendGameData.Instance.userData.UpdatePlayerItemAt((int)type, v);
        UniquePlayer.InitItem();
    }
   
}
