using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour
{
    // 슬롯 데이터 클래스
    [System.Serializable]
    public class InventoryData
    {
        public int slotID;
        public string itemName;
        public Sprite itemIcon;
        public bool isEquipped;
    }

    // 슬롯 데이터 리스트
    [SerializeField]
    public List<InventoryData> inventoryDataList = new List<InventoryData>();

    // 탭 부모 리스트 (Inspector에서 할당)
    [SerializeField]
    private List<GameObject> tabParents; // 예: Tab1, Tab2, Tab3

    // 아이템 프리팹 (Inspector에서 할당)
    [SerializeField]
    private GameObject itemPrefab;

    // 모든 슬롯을 저장할 리스트
    private List<GameObject> allSlots = new List<GameObject>();

    // 클릭 카운트 및 코루틴 관리를 위한 딕셔너리
    private Dictionary<int, int> slotClickCounts = new Dictionary<int, int>();
    private Dictionary<int, Coroutine> slotCoroutines = new Dictionary<int, Coroutine>();

    [SerializeField]
    private float doubleClickThreshold = 0.3f; // 더블 클릭 인식 시간 간격 (초)

    void Start()
    {
        InitializeSlots();
    }

    // 슬롯 초기화 및 아이템 프리팹 추가 메서드
    private void InitializeSlots()
    {
        int slotID = 0;

        foreach (GameObject tabParent in tabParents)
        {
            foreach (Transform slotTransform in tabParent.transform)
            {
                // 슬롯 이름 설정 (선택 사항)
                slotTransform.gameObject.name = $"Slot_{slotID}";

                // 아이템 프리팹을 슬롯의 자식으로 인스턴스화
                GameObject itemInstance = Instantiate(itemPrefab, slotTransform);
                itemInstance.transform.localPosition = Vector3.zero; // 위치 초기화
                itemInstance.transform.localScale = Vector3.one;     // 스케일 초기화

                // 슬롯을 리스트에 추가
                allSlots.Add(slotTransform.gameObject);

                // 슬롯 데이터 초기화 및 추가
                InventoryData data = new InventoryData
                {
                    slotID = slotID,
                    itemName = $"Item {slotID + 1}",
                    itemIcon = null, // 필요한 경우 아이콘 설정
                    isEquipped = false
                };
                inventoryDataList.Add(data);

                // 아이템 프리팹에 클릭 이벤트 등록
                AddClickEventListener(itemInstance, slotID);

                slotID++;
            }
        }
    }

    // 아이템 프리팹에 클릭 이벤트 리스너 추가
    private void AddClickEventListener(GameObject itemInstance, int slotID)
    {
        EventTrigger trigger = itemInstance.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = itemInstance.AddComponent<EventTrigger>();
        }

        // 클릭 이벤트 등록
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((eventData) => { OnItemClicked((PointerEventData)eventData, slotID); });
        trigger.triggers.Add(entry);
    }

    // 아이템이 클릭되었을 때 호출되는 메서드
    private void OnItemClicked(PointerEventData eventData, int slotID)
    {
        if (!slotClickCounts.ContainsKey(slotID))
        {
            slotClickCounts[slotID] = 0;
        }

        slotClickCounts[slotID]++;

        // 기존에 실행 중인 코루틴이 있다면 중지
        if (slotCoroutines.ContainsKey(slotID))
        {
            StopCoroutine(slotCoroutines[slotID]);
        }

        // 새로운 코루틴 시작
        slotCoroutines[slotID] = StartCoroutine(HandleClicks(slotID));
    }

    // 클릭을 처리하는 코루틴
    private IEnumerator HandleClicks(int slotID)
    {
        yield return new WaitForSeconds(doubleClickThreshold);

        if (slotClickCounts[slotID] == 1)
        {
            // 단일 클릭 처리
            OnSingleClick(slotID);
        }
        else if (slotClickCounts[slotID] == 2)
        {
            // 더블 클릭 처리
            OnDoubleClick(slotID);
        }

        // 클릭 카운트 초기화
        slotClickCounts[slotID] = 0;
    }

    // 단일 클릭 처리 메서드
    private void OnSingleClick(int slotID)
    {
        Debug.Log($"Slot {slotID} Single Clicked");
        ShowTooltip(slotID);
    }

    // 더블 클릭 처리 메서드
    private void OnDoubleClick(int slotID)
    {
        Debug.Log($"Slot {slotID} Double Clicked");
        EquipItem(slotID);
    }

    // 툴팁 표시 메서드
    private void ShowTooltip(int slotID)
    {
        // 툴팁 표시 로직 구현
        // 예: TooltipManager.Instance.Show(slotID, inventoryDataList[slotID].itemName);
    }

    // 아이템 장착 메서드
    private void EquipItem(int slotID)
    {
        if (slotID >= 0 && slotID < inventoryDataList.Count)
        {
            InventoryData data = inventoryDataList[slotID];
            data.isEquipped = !data.isEquipped; // 토글 예제

            Debug.Log($"Item in Slot {slotID} Equipped: {data.isEquipped}");

            // 슬롯 UI 업데이트
            UpdateSlotUI(slotID);
        }
    }

    // 슬롯 UI 업데이트 메서드
    private void UpdateSlotUI(int slotID)
    {
        if (slotID >= 0 && slotID < allSlots.Count)
        {
            GameObject slot = allSlots[slotID];
            Transform itemTransform = slot.transform.Find(itemPrefab.name);

            if (itemTransform != null)
            {
                Image itemImage = itemTransform.GetComponent<Image>();
                if (itemImage != null)
                {
                    // 장착 상태에 따라 아이콘 색상 변경
                    itemImage.color = inventoryDataList[slotID].isEquipped ? Color.green : Color.white;

                    // 아이콘 스프라이트 업데이트 (필요 시)
                    if (inventoryDataList[slotID].itemIcon != null)
                    {
                        itemImage.sprite = inventoryDataList[slotID].itemIcon;
                    }
                }
            }
        }
    }
}