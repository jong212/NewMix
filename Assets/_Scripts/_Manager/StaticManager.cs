using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


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
            currentAction?.Invoke(); // 작업 실행
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


    public void SetInvenItemSwap(int changeA, int ChangeB)
    {
        EnqueueAction(() =>
        {
            SetInvenItemSwapQueue(changeA, ChangeB);
        });
    }
    private void SetInvenItemSwapQueue(int beforeSloatId, int afterSloatId)
    {

        foreach(Mymon mon in CashUdata.mymonList)
        {
            if(mon.mList.Count > 0 && "mymon" + (beforeSloatId + 1) == mon.columName )
            {
                mon.columName = "mymon" + (afterSloatId + 1);
                BackendGameData.Instance.GameDataUpdate<List<int>>(mon.columName, mon.mList);
                continue;
            }
            if (mon.mList.Count > 0 && "mymon" + (afterSloatId + 1) == mon.columName)
            {
                mon.columName = "mymon" + (beforeSloatId + 1);
                BackendGameData.Instance.GameDataUpdate<List<int>>(mon.columName, mon.mList);
            }
        }   
    }
    public void SetInvenItemMove(int changeA, int ChangeB)
    {
        EnqueueAction(() =>
        {
            SetInvenItemMoveQueue(changeA, ChangeB);
        });
    }
    /// <summary>
    /// 찝은 것을 빈 슬롯에 놓았을 때 실행되는 메서드
    /// </summary>
    /// <param name="beforeSloatId">찝은 슬롯의 인덱스 값</param>
    /// <param name="afterSloatId">놓은 슬롯의 인덱스 값</param>
    private void SetInvenItemMoveQueue(int beforeSloatId, int afterSloatId)
    {
        Mymon temp = null;

        // 1. 기존 슬롯의 데이터는 백업 한다.
        // 2. 기존 슬롯의 데이터 백업 후 0으로 세팅 한다.
        for (int i = 0; i < CashUdata.mymonList.Count; i++)
        {
            Mymon mon = CashUdata.mymonList[i];
            if (mon.mList.Count > 0 && "mymon" + (beforeSloatId + 1) == mon.columName)
            {
                // 1. 백업
                temp = new Mymon
                {
                    columName = mon.columName,
                    mList = new List<int>(mon.mList) // 깊은 복사
                };

                // 2. 백업 해두었으니 0으로 세팅
                mon.mList = new List<int> { 0 };
                BackendGameData.Instance.GameDataUpdate<List<int>>(mon.columName, mon.mList);
                break; // 첫 번째 조건을 만족했으므로 탈출
            }
        }

        // 두 번째 조건: afterSloatId에 해당하는 항목 업데이트
        if (temp != null) // temp가 null이 아니어야 작업 진행
        {
            for (int i = 0; i < CashUdata.mymonList.Count; i++)
            {
                Mymon mon = CashUdata.mymonList[i];
                if (mon.mList.Count > 0 && "mymon" + (afterSloatId + 1) == mon.columName)
                {
                    // 값 업데이트
                    mon.mList = new List<int>(temp.mList);  
                    BackendGameData.Instance.GameDataUpdate<List<int>>(mon.columName, mon.mList);
                    break; // 작업 완료 후 탈출
                }
            }
        }
    }

    public void SetSubInvenToInven(InventoryType type, int ChangeB)
    {
        EnqueueAction(() =>
        {
            SetSubInvenToInvenQueue(type, ChangeB);
        });
    }
    private void SetSubInvenToInvenQueue(InventoryType type, int afterSloatId)
    {
        // 참조 캐싱
        var tempUserdata = BackendGameData.Instance.userData;

        Match match = Regex.Match(type.ToString(), @"\d+");
        int number = int.Parse(match.Value); // 숫자 변환

        foreach (SetMymon setCulum in tempUserdata.setMymonList)
        {
            if(setCulum.columName == type.ToString())
            {
                string tempCulName = setCulum.columName;

                foreach(var dataChange in CashUdata.mymonList)
                {
                    if(dataChange.columName == "mymon" + (afterSloatId + 1))
                    {
                        StaticManager.UI.MainUI.MonUIList[number - 1].resetObject();
                        dataChange.mList = setCulum.setMonList;
                        BackendGameData.Instance.GameDataUpdate<List<int>>(dataChange.columName, dataChange.mList);
                        BackendGameData.Instance.GameDataUpdate<List<int>>(tempCulName, new List<int> { 0 });
                        setCulum.setMonList = new List<int> { 0 };
                        return;
                    }
                }

            }
        } 
    }
    public void SetDoubleClickItem(InventoryType type, int subInvenIdx)
    {
        EnqueueAction(() =>
        {
            SetDoubleClickItemQueue(type, subInvenIdx);
        });
    }
    void SetDoubleClickItemQueue(InventoryType type, int subInvenIdx)
    {
        Mymon temp = null;
        for (int i = 0; i < CashUdata.mymonList.Count; i++)
        {
            Mymon mon = CashUdata.mymonList[i];
            if (mon.mList.Count > 0 && "mymon" + (subInvenIdx + 1) == mon.columName)
            {
                // 1. 백업
                temp = new Mymon
                {
                    columName = mon.columName,
                    mList = new List<int>(mon.mList) // 깊은 복사
                };

                // 2. 백업 해두었으니 0으로 세팅
                mon.mList = new List<int> { 0 };
                BackendGameData.Instance.GameDataUpdate<List<int>>(mon.columName, mon.mList);
                break; // 첫 번째 조건을 만족했으므로 탈출
            }
        }

        if (temp != null) // temp가 null이 아니어야 작업 진행
        {
            for (int i = 0; i < CashUdata.setMymonList.Count; i++)
            {
                SetMymon mon = CashUdata.setMymonList[i];
                if (mon.setMonList.Count > 0 && type.ToString() == mon.columName)
                {
                    // 값 업데이트
                    mon.setMonList = new List<int>(temp.mList);
                    BackendGameData.Instance.GameDataUpdate<List<int>>(mon.columName, mon.setMonList);
                    GameManager.instance.SpawnMonsterData();
                    break; // 작업 완료 후 탈출
                }
            }
        }
    }



    public bool GetDropItemAction(int itemNumber)
    {
        bool b = default;
        EnqueueAction(() =>
        {
            b = GetDropItem(itemNumber);
        });
        return b;
    }
    private bool GetDropItem(int itemNumber)
    {
        List<InventorySlot> tempSloat = CashUdata.InventorySlots;
        foreach ( var item in tempSloat)
        {
            if(item.ItemId == null)
            {
                item.ItemId = itemNumber;
                UI.ContentsInventoryUI.updateSloat(item.SlotId, itemNumber);
                Debug.Log(item.SlotId + " 이SloatID는 널이였고, 얻은 아이템은"+ itemNumber + "였음");
                string inventoryJson = JsonMapper.ToJson(new { slots = CashUdata.InventorySlots });
                BackendGameData.Instance.GameDataUpdate<string>("Inventory", inventoryJson);
                return false;
            }
        }
        return true;
    }

}
