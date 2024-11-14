using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Btn : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private float doubleClickThreshold = 0.3f; // 더블 클릭을 인식할 시간 간격

    private int clickCount = 0;                // 클릭 시 
    private Coroutine clickCoroutine;

    public void OnPointerClick(PointerEventData eventData)
    {
        clickCount++;

        if (clickCoroutine != null)
        {
            StopCoroutine(clickCoroutine);
        }

        clickCoroutine = StartCoroutine(HandleClicks());
    }

    private IEnumerator HandleClicks()
    {
        yield return new WaitForSeconds(doubleClickThreshold);

        if (clickCount == 1)
        {
            // 단일 클릭 처리
            OnSingleClick();
        }
        else if (clickCount == 2)
        {
            // 더블 클릭 처리
            OnDoubleClick();
        }

        clickCount = 0;
    }

    private void OnSingleClick()
    {
        Debug.Log("Single Click");
        ShowTooltip();
    }

    private void OnDoubleClick()
    {
        Debug.Log("Double Click");
        EquipItem();
    }

    private void ShowTooltip()
    {
        // 툴팁 표시 로직 구현
        // 예: tooltip.SetActive(true);
    }

    private void EquipItem()
    {
        // 아이템 장착 로직 구현
        Debug.Log("Item Equipped!");
        // 예: tooltip.SetActive(false);
    }
}
