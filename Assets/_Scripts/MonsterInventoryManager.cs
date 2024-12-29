using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using ExitGames.Client.Photon;

public class MonsterInventoryManager : MonoBehaviour
{

    [SerializeField] private List<Transform> tabParents; // 예: Tab1, Tab2, Tab3

    [SerializeField] private List<DroppableUI> subInventory; // 예: 장비 착용 창
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private float doubleClickThreshold = 0.3f; // 더블 클릭 인식 시간 간격 (초)

    [SerializeField] private Text _name;
    [SerializeField] private Text _lv;
    [SerializeField] private Text _power;
    [SerializeField] private Text _def;
    [SerializeField] private Text _hp; 
    [SerializeField] private Sprite _btnClickImg;
    [SerializeField] private Sprite _btnNoClickImg;

    public Text Name { get => _name; set => _name = value; }
    public Text Lv { get => _lv; set => _lv = value; }
    public Text Power { get => _power; set => _power = value; }
    public Text Def { get => _def; set => _def = value; }
    public Text Hp { get => _hp; set => _hp = value; }
 

    private List<Transform> allSlots = new List<Transform>(); // 모든 슬롯을 저장할 리스트
    private Dictionary<int, int> slotClickCounts = new Dictionary<int, int>(); // 클릭 카운트 및 코루틴 관리를 위한 딕셔너리
    private Dictionary<int, Coroutine> slotCoroutines = new Dictionary<int, Coroutine>();

    public bool SceneChangeInit = false;
    public void FirstInit()
    {
        if (!SceneChangeInit)
        {
            SceneChangeInit = true;
        }
        else
        {
            gameObject.SetActive(false);
            return;
        }
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
        List<Mymon> sData = BackendGameData.Instance.userData.mymonList;
        List<MonsterInfoChart> itemChart = BackendGameData.Instance.MonsterInfoList;
        foreach (Mymon slotClass in sData)
        {
            Match match = Regex.Match(slotClass.columName, @"\d+");
            int number = int.Parse(match.Value); // 숫자 변환

            GameObject itemInstance = Instantiate(itemPrefab, allSlots[number - 1]);
            itemInstance.transform.localPosition = Vector3.zero; // 위치 초기화
            itemInstance.transform.localScale = Vector3.one;     // 스케일 초기화

           if (itemInstance.TryGetComponent(out Btn component))
            {
                // 아이템 프리팹을 슬롯의 자식으로 인스턴스화
                foreach (MonsterInfoChart item in itemChart)
                {
                    if (slotClass.mList.Count > 0 && slotClass.mList[0] == (int)item.MonsterId )
                    {
                        Sprite spriteImg = AddressableManager.instance.GetSprite(item.MyMonSpriteName);
                        if (spriteImg != null)
                        {
                            //몬스터아이디, 레벨, 공격력,방어력,체력,공격범위

                            component.SpriteImg = spriteImg;
                            component.ActiveChk = true;
                            component.Lv = slotClass.mList[1];
                            component.Str = slotClass.mList[2];
                            component.Def = slotClass.mList[3];
                            component.Hp = slotClass.mList[4];
                            component.Name = item.MonsterName;
                        }
                       
                        break;
                    }
                }

                component.myMonsterManager = this; // Pass the InventoryManager reference
            }
        }
        List<SetMymon> setMon = BackendGameData.Instance.userData.setMymonList;

        int tIdx = 0;        
        foreach (SetMymon setInvenIdx in setMon)
        {
            GameObject itemInstance = Instantiate(itemPrefab, subInventory[tIdx].transform);
            if (itemInstance.TryGetComponent(out Btn component))
            {
                foreach (MonsterInfoChart item in itemChart)
                {
                    if (setInvenIdx.setMonList.Count > 0 && setInvenIdx.setMonList[0] == (int)item.MonsterId)
                    {
                        Sprite spriteImg = AddressableManager.instance.GetSprite(item.MyMonSpriteName);
                        if (spriteImg != null)
                        {
                            component.SpriteImg = spriteImg;
                            component.ActiveChk = true;
                            component.Lv = setInvenIdx.setMonList[1];
                            component.Str = setInvenIdx.setMonList[2];
                            component.Def = setInvenIdx.setMonList[3];
                            component.Hp = setInvenIdx.setMonList[4];
                            component.Name = item.MonsterName;
                        }
                        break;
                    }
                }
                component.myMonsterManager = this; // Pass the InventoryManager reference
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
        slotCoroutines[slotID] = StartCoroutine(HandleClicks(slotID, eventData));
    }

    // 클릭을 처리하는 코루틴
    private IEnumerator HandleClicks(int slotID, PointerEventData eventData)
    {
        yield return new WaitForSeconds(doubleClickThreshold);

        if (slotClickCounts[slotID] == 1)
        {

            if(slotID == null || eventData == null)
            {
                slotClickCounts[slotID] = 0;
                yield break;
            }
            // 단일 클릭 처리
            OnSingleClick(eventData);
        }
        else if (slotClickCounts[slotID] == 2)
        {
            if (slotID == 100 || slotID == 101 || slotID == 102 || slotID == 103) yield break;

            // 더블 클릭 처리
            if (eventData.lastPress.TryGetComponent(out Btn component))
            {
                // 인벤에서 더블 클릭한 아이템을 장비창에 낄 것인지 검증하는 로직을 여기쯤 작성해야함 
                foreach (var subidx in subInventory)
                {
                    Btn slotItem = subidx.GetComponentInChildren<Btn>();
                    if(slotItem != null)
                    {
                        if(slotItem.ActiveChk)
                        {
                            continue;
                        } else
                        {
                            StaticManager.Instance.SetDoubleClickItem(subidx.InventoryType, slotID);
                            slotItem.transform.SetParent(component.transform.parent.transform);
                            slotItem.GetComponent<RectTransform>().localPosition = Vector3.zero;
                            component.transform.SetParent(subidx.transform);
                            component.GetComponent<RectTransform>().localPosition = Vector3.zero;
                            break;
                        }
                    }
                }
            }
            OnDoubleClick(slotID);
        }

        // 클릭 카운트 초기화
        slotClickCounts[slotID] = 0;
    }

    // 단일 클릭 처리 메서드
    private void OnSingleClick(PointerEventData eData)
    {
        if(eData.lastPress == null)
        {
            return;
        } 
        if (eData.lastPress.TryGetComponent(out Btn component))
        {
            if (!component.ActiveChk) return;
            Name.text = component?.Name.ToString();
            Lv.text = component?.Lv.ToString();
            Power.text = component?.Str.ToString();
            Def.text = component?.Def.ToString();
            Hp.text = component?.Hp.ToString();
        } else
        {
            Debug.Log("?D?D");
        }
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
}