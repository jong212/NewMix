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
    [SerializeField] int _lv;
    [SerializeField] int _str;
    [SerializeField] int _def;
    [SerializeField] int _hp;
    [SerializeField] float _attackSpeed;
    [SerializeField] float _moveSpeed;
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
    public int Lv  { get => _lv;   set => _lv = value; }
    public int Str  { get => _str;   set => _str = value; }
    public int Def  { get => _def;   set => _def = value; }
    public int Hp  { get => _hp;   set => _hp = value; }
    public float AttackSpeed  { get => _attackSpeed;   set => _attackSpeed = value; }
    public float MoveSpeed  { get => _moveSpeed;   set => _moveSpeed = value; }
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
