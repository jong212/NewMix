using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonExpHpMpContainer : MonoBehaviour
{
    private Mentity _Mymonster;
    public bool SetObjectCheck { get; private set; }

    [Header("mon1")]
    [SerializeField] private Image _mon1ExpBar;
    [SerializeField] private Image _mon1Hp;
    [SerializeField] private Image _mon1Mp;
    [SerializeField] private Image _mon1ProfileImage;

    void Awake()
    {
    }

    private void OnEnable()
    {
     
 
    }
    public void SetProfileImg(Sprite sprite)
    {
        _mon1ProfileImage.sprite = sprite;
        // 기존의 색상 값을 가져오고 알파 값만 수정
        Color currentColor = _mon1ProfileImage.color;
        currentColor.a = 1.0f;  // 알파 값을 1로 설정 (불투명)

        // 변경된 색상을 다시 적용
        _mon1ProfileImage.color = currentColor;
    }
    public void init(Mentity enemy)
    {
        _Mymonster = enemy;
        _Mymonster.OnStatsChanged += UpdateStats;
        _Mymonster.InitHpUpdate();
        _mon1Mp.fillAmount = 1;
        SetObjectCheck = true;
    }
    public void resetObject()
    {
        SetObjectCheck = false;

        _Mymonster.OnStatsChanged -= UpdateStats;
        _Mymonster.Despawn();
        _mon1Hp.fillAmount = 0;
        _mon1Mp.fillAmount = 0;
        _mon1ProfileImage.sprite = null;
        Color currentColor = _mon1ProfileImage.color;
        currentColor.a = 0;
        _mon1ProfileImage.color = currentColor;


    }
    private void OnDisable()
    {
        if(_Mymonster != null)
        {
            _Mymonster.OnStatsChanged -= UpdateStats;
        }
        

    }
    private void UpdateStats()
    {
        _mon1Hp.fillAmount = (float)_Mymonster.CurrentHp / _Mymonster.Hp;
    }
}
