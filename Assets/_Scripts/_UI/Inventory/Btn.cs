using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Btn : MonoBehaviour, IPointerClickHandler
{

    [SerializeField] private bool _activeChk;
    [SerializeField] Image _img;
    [SerializeField] string _category;
    [SerializeField] string _name;
    [SerializeField] string _lv;
    [SerializeField] string _str;
    [SerializeField] string _def;
    [SerializeField] string _hp;
    public Sprite SpriteImg { 
        get => _img.sprite; 
        set 
        { 
            _img.sprite = value;
            Color test = _img.color;
            test.a = 1;
            _img.color = test;
        }
    }
    public bool   ActiveChk { get => _activeChk;  set => _activeChk  = value; }
    public string Category  { get => _category;   set => _category   = value; }
    public string Name  { get => _name;   set => _name = value; }
    public string Lv  { get => _lv;   set => _lv = value; }
    public string Str  { get => _str;   set => _str = value; }
    public string Def  { get => _def;   set => _def = value; }
    public string Hp  { get => _hp;   set => _hp = value; }
    public InventoryManager ivtmanager;
    public MonsterInventoryManager myMonsterManager;


    public void OnPointerClick(PointerEventData eventData)
    {
        if (ivtmanager != null)
        {
            ivtmanager.OnItemClicked(eventData, transform.GetComponentInParent<DroppableUI>().Idx);
        } else
        {
            myMonsterManager.OnItemClicked(eventData, transform.GetComponentInParent<DroppableUI>().Idx);
        }
    }

}
