using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DroppableUI : MonoBehaviour, IPointerEnterHandler, IDropHandler, IPointerExitHandler
{
	private Image image;
	private RectTransform rect;
/*	[SerializeField] InventoryType inventorytype;
*/    [SerializeField] int idx;
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
			/*Debug.Log(eventData.pointerDrag.GetComponent<DraggableUI>().PreviousParent.name + "이전 슬롯 Name");
			Debug.Log(gameObject.name + "놓은 슬롯 Name");*/

            DraggableUI DragingPrefab = eventData.pointerDrag.GetComponent<DraggableUI>();

            if (DragingPrefab.PreviousParent.name.Contains("Sloat"))			// 드래그 하기 전 부모 오브젝트 이름이 Sloat인지 즉, 장비가 아닌 인벤토리 슬롯인지
			{
				if (gameObject.name.Contains("Sloat"))							//  오브젝트 놓은 위치가 슬롯인 경우에만 
				{
					if(DragingPrefab.PreviousParent.name != gameObject.name)	// 오브젝트 잡고 놓은 위치가 같지 않은 경우에만
					{
                        if(gameObject.GetComponentInChildren<Btn>()?.ActiveChk == true)
						{
							StaticManager.Instance.InvenSortTwoChange(DragingPrefab.PreviousParent.GetComponent<DroppableUI>().idx,Idx);
                        } else
						{
                            StaticManager.Instance.InvenSortOneMove(DragingPrefab.PreviousParent.GetComponent<DroppableUI>().idx, Idx);
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
			// 드래그하고 있는 대상의 부모를 현재 오브젝트로 설정하고, 위치를 현재 오브젝트 위치와 동일하게 설정

		}
	}
}

