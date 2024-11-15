using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Btn : MonoBehaviour, IPointerClickHandler
{
    public Sprite img;
    public InventoryManager ivtmanager;


    public void OnPointerClick(PointerEventData eventData)
    {
        if (ivtmanager != null)
        {
            ivtmanager.OnItemClicked(eventData, transform.GetComponentInParent<DroppableUI>().Idx);
        }
    }
}
