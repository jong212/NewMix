using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Btn : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private bool _activeChk;
    [SerializeField] Image _img;
    [SerializeField] string _category;
    public Sprite SpriteImg { get => _img.sprite; set => _img.sprite = value; }
    public bool   ActiveChk { get => _activeChk;  set => _activeChk  = value; }
    public string Category  { get => _category;   set => _category   = value; }
    public InventoryManager ivtmanager;


    public void OnPointerClick(PointerEventData eventData)
    {
        if (ivtmanager != null)
        {
            ivtmanager.OnItemClicked(eventData, transform.GetComponentInParent<DroppableUI>().Idx);
        }
    }
}
