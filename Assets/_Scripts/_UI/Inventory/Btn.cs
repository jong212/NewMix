using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Btn : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Image _img;
    public Sprite spriteImg
    {
        get => _img.sprite;
        set => _img.sprite = value;
        
    }
    public InventoryManager ivtmanager;


    public void OnPointerClick(PointerEventData eventData)
    {
        if (ivtmanager != null)
        {
            ivtmanager.OnItemClicked(eventData, transform.GetComponentInParent<DroppableUI>().Idx);
        }
    }
}
