using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using static UnityEditor.Progress;
using System.ComponentModel;
/*public enum InventoryType
{
    Weapon = 100,
    Armor = 101,
    Gluve = 102,
    Shose = 103,
}*/
public class InventoryManager : MonoBehaviour   
{
     
     // 테스트 ㅁㅁㅁㅁㅁ 
    // 탭 부모 리스트 (Inspector에서 할당)
    [SerializeField]    private List<Transform> tabParents; // 예: Tab1, Tab2, Tab3
    [SerializeField]    private List<DroppableUI> subInventory; // 예: 장비 착용 창

    // 아이템 프리팹 (Inspector에서 할당)
    [SerializeField]
    private GameObject itemPrefab;

    // 모든 슬롯을 저장할 리스트
    private List<Transform> allSlots = new List<Transform>();

    // 클릭 카운트 및 코루틴 관리를 위한 딕셔너리
    private Dictionary<int, int> slotClickCounts = new Dictionary<int, int>();
    private Dictionary<int, Coroutine> slotCoroutines = new Dictionary<int, Coroutine>();

    [SerializeField]
    private float doubleClickThreshold = 0.3f; // 더블 클릭 인식 시간 간격 (초)

    void Start()
    {
        InitMergeSloat();
        InitializeSlots();
        gameObject.SetActive(false);
    }
    
    private void InitMergeSloat()
    {
       allSlots = tabParents.SelectMany(tab => tab.Cast<Transform>()).ToList();
    }

    // 슬롯 초기화 및 아이템 프리팹 추가 메서드
    private void InitializeSlots()
    {
        List<InventorySlot> sData = BackendGameData.Instance.userData.InventorySlots;
        List<ItemChart> itemChart =  BackendGameData.Instance.ItemChartList;
        foreach (InventorySlot slotClass in sData)
        {
            GameObject itemInstance = Instantiate(itemPrefab, allSlots[slotClass.SlotId - 1]);
            itemInstance.transform.localPosition = Vector3.zero; // 위치 초기화
            itemInstance.transform.localScale = Vector3.one;     // 스케일 초기화

            if (itemInstance.TryGetComponent(out Btn component))
            {
                // 아이템 프리팹을 슬롯의 자식으로 인스턴스화
                foreach (ItemChart item in itemChart )
                {
                    if(slotClass.ItemId == item.Itemid)
                    {
                        Sprite spriteImg = AddressableManager.instance.GetSprite(item.SpriteName);
                        if(spriteImg != null)
                        {
                            component.SpriteImg = spriteImg;
                            component.ActiveChk = true;
                        }
                    break;
                    }
                }

                component.ivtmanager = this; // Pass the InventoryManager reference
            }
        }

        List<int> setPlayeritem = BackendGameData.Instance.userData.setPlayerItems;
        int tIdx = 0;
        foreach (int setInvenIdx in setPlayeritem)
        {
            GameObject itemInstance = Instantiate(itemPrefab, subInventory[tIdx].transform);
            if(itemInstance.TryGetComponent(out Btn component))
            {
                foreach (ItemChart item in itemChart)
                {
                    if (setInvenIdx == item.Itemid)
                    {
                        Sprite spriteImg = AddressableManager.instance.GetSprite(item.SpriteName);
                        if (spriteImg != null)
                        {
                            component.SpriteImg = spriteImg;
                            component.ActiveChk = true;
                        }
                    }
                }
            }

            tIdx++;

        }
    }

    // 아이템이 클릭되었을 때 호출되는 메서드
    public void OnItemClicked(PointerEventData eventData, int slotID)
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

    }

    // 슬롯 UI 업데이트 메서드
    private void UpdateSlotUI(int slotID)
    {

    }
    private void OnEnable()
    {
        tabParents[0].gameObject.SetActive(true);
        tabParents[1].gameObject.SetActive(false);
        tabParents[2].gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        tabParents[0].gameObject.SetActive(true);
        tabParents[1].gameObject.SetActive(false);
        tabParents[2].gameObject.SetActive(false);
    }
}