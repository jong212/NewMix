using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public enum InventoryType
{
    Weapon = 0,
    Armor = 1,
    Gluve = 2,
    Shose = 3,
	MyMonster = 4,
	SetMymon1 = 5,
    SetMymon2 = 6,
    SetMymon3 = 7
}
public class DroppableUI : MonoBehaviour, IPointerEnterHandler, IDropHandler, IPointerExitHandler
{
	private Image image;
	private RectTransform rect;
	[SerializeField] InventoryType _inventorytype;
	public InventoryType InventoryType { get => _inventorytype; }
    [SerializeField] int idx;
	public int Idx { get => idx; }
	private void Awake()
	{
		image	= GetComponent<Image>();
		rect	= GetComponent<RectTransform>();
	}

	/// <summary>
	/// 마우스 포인트가 현재 아이템 슬롯 영역 내부로 들어갈 때 1회 호출
	/// </summary>
	public void OnPointerEnter(PointerEventData eventData)
	{
		// 아이템 슬롯의 색상을 노란색으로 변경
		image.color = Color.yellow;
	}

	/// <summary>
	/// 마우스 포인트가 현재 아이템 슬롯 영역을 빠져나갈 때 1회 호출
	/// </summary>
	public void OnPointerExit(PointerEventData eventData)
	{
		// 아이템 슬롯의 색상을 하얀색으로 변경
		image.color = Color.white;
	}

	/// <summary>
	/// 현재 아이템 슬롯 영역 내부에서 드롭을 했을 때 1회 호출
	/// </summary>
	public void OnDrop(PointerEventData eventData)
	{
         
        // pointerDrag는 현재 드래그하고 있는 대상(=아이템)
        if ( eventData.pointerDrag != null )
		{
            DraggableUI DragingPrefab = eventData.pointerDrag.GetComponent<DraggableUI>();
			if (DragingPrefab.PreviousParent == null) return;
			// 인벤창에서 드래그 시작한 경우 (장비 X)
            if (DragingPrefab && DragingPrefab.PreviousParent.name.Contains("Sloat"))			
			{
				// 인벤 에서 인벤
				if (gameObject.name.Contains("Sloat"))							
				{
					// 인벤의 슬롯을 드래그 하였지만 제자리에 그냥 놓은 경우 실행 X
					if(DragingPrefab.PreviousParent.name != gameObject.name)	
					{
						if(InventoryType == InventoryType.MyMonster)
						{
                            if (gameObject.GetComponentInChildren<Btn>()?.ActiveChk == true)
                            {
                                StaticManager.Instance.SetInvenItemSwap(DragingPrefab.PreviousParent.GetComponent<DroppableUI>().idx, Idx);
                            } else
                            {
                                StaticManager.Instance.SetInvenItemMove(DragingPrefab.PreviousParent.GetComponent<DroppableUI>().idx, Idx);
                            }
                        } else
						{
                            if (gameObject.GetComponentInChildren<Btn>()?.ActiveChk == true)
                            {
                                StaticManager.Instance.InvenItemSwap(DragingPrefab.PreviousParent.GetComponent<DroppableUI>().idx, Idx);
                            }
                            else
                            {
                                StaticManager.Instance.InvenItemMove(DragingPrefab.PreviousParent.GetComponent<DroppableUI>().idx, Idx);
                            }
                        }


                    } else
					{
						return;
					}
				} else
				{
					if(eventData.pointerDrag.GetComponent<Btn>()?.Category != InventoryType.ToString())
					{
						return;
					}
					return; // 인벤창에서 장비창으로 드래그햇을 떄 DB처리를 여기에 해야하는데 작업 량이 많아져서 그냥 return; 시킴 어차피 더블클릭으로 장비 창용 가능
                    
				}
			} //장비창에서 드래그 시작했고 인벤창에 놓은 경우
			else if (DragingPrefab && DragingPrefab.PreviousParent.name.Contains("EquipmentShot") && transform.name.Contains("Sloat")) 
			{	// 장비창 아이템을 인벤창에 놓았는데 빈 슬롯인 경우에만 실행 되도록
				 if (gameObject.GetComponentInChildren<Btn>()?.ActiveChk == false)
				{
                    if (DragingPrefab.PreviousParent.GetComponent<DroppableUI>().InventoryType == InventoryType.SetMymon1 ||
                        DragingPrefab.PreviousParent.GetComponent<DroppableUI>().InventoryType == InventoryType.SetMymon2 ||
                         DragingPrefab.PreviousParent.GetComponent<DroppableUI>().InventoryType == InventoryType.SetMymon3)
					{
                        StaticManager.Instance.SetSubInvenToInven(DragingPrefab.PreviousParent.GetComponent<DroppableUI>().InventoryType, Idx);

					} else
					{
                        StaticManager.Instance.SubInvenToInven(DragingPrefab.PreviousParent.GetComponent<DroppableUI>()._inventorytype, Idx);
					}
                }
			} else
			{
				return;
			}
            Transform btn = gameObject.GetComponentInChildren<Btn>().transform;

            // 마우스 뗀 위치의 자식 아이템을 => 마우스 클릭한 위치의 자식으로 이동
            btn.SetParent(DragingPrefab.PreviousParent.transform);
            btn.GetComponent<RectTransform>().localPosition = Vector3.zero;

            // 위와 반대 (클릭한 아이템을 클릭 뗀 슬롯의 자식으로)
            eventData.pointerDrag.transform.SetParent(transform);
            eventData.pointerDrag.GetComponent<RectTransform>().position = rect.position;
            // TO DO 놓은 위치의 오브젝트 자식에 아이템이 있는지? 있으면 Swap 없으면 이동

        }
    }
}

