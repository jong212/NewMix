using BackEnd;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.TextCore.Text;


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

    void Awake()
    {
        Init();
    }
    private void Update()
    {
        //Debug.Log(CashUdata?.ToString());
        
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
            yield return null; // 다음 프레임까지 대기
        }
        isProcessing = false;
    }

    // 모바일에서 오브젝트 위치 확인용
    public void LogHierarchyPath(Transform transform)
    {
        if (transform == null)
        {
            Debug.LogError("Transform is null. Cannot log hierarchy path.");
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

        // 이름과 경로를 로그로 출력
        Debug.Log($"Object name: {transform.name}, Path: {path}");
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
        Matchmaker.Instance.Runner.Shutdown();

        // 잠시 대기하여 Runner가 완전히 정리될 시간을 준다
        yield return new WaitForSeconds(1);
        Matchmaker.Instance.TryConnectShared();
    }
     public void InvenSortTwoChange(int changeA, int ChangeB)
    {
        EnqueueAction(() =>
        {
            Sort(changeA, ChangeB);
        });
    }
    private void Sort(int beforeSloatId, int afterSloatId)
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
}
