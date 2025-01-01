using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ExpHpMpContainer : MonoBehaviour
{
    private Character character;

    [Header("Player")]
    [SerializeField] private GameObject _exp;
    [SerializeField] private Image _playerExpBar;
    [SerializeField] private Image _playerHp;
    [SerializeField] private Image _playerMp;
    [SerializeField] private Image _playerProfileImage; 

    void Awake()
    {
    }

    private void OnEnable()
    {
       
    } 
    public void init()
    {
        _exp.gameObject.SetActive(true);
        character = StaticManager.Instance.UniquePlayer;
        character.OnStatsChanged += UpdateStats;
        character.OnExpChanged += UpdateExp;
        character.InitHpUpdate();

        int cType = BackendGameData.Instance.userData.ChrType;

        foreach (CharacterSrcChart character in BackendGameData.Instance.CharacterList) // 차트 데이터
        {
            if (cType == character.charId)
            {
                string tempCharactername = character.profileSpriteName;
                if(tempCharactername != null)
                {
                    _playerProfileImage.sprite = AddressableManager.instance.GetSprite(tempCharactername);
                    Color color = _playerProfileImage.color;
                    color.a = 1f;
                    _playerProfileImage.color = color;
                }
                return ;
            }
        }

    }
    private void OnDisable()
    {
       character.OnStatsChanged -= UpdateStats;
       character.OnExpChanged -= UpdateExp;

    }
    private void UpdateStats()
    {
        _playerHp.fillAmount = (float)character.CurrentHp / character.FinalHP;
    }
    private void UpdateExp()
    {
         foreach (var lvKey in character.ExpInfo)
        {
            if (lvKey.Key == character.Level) // 현재 레벨과 일치하는 레벨 찾기
            {
                    _playerExpBar.fillAmount = (float)character.CurExp / lvKey.Value;
            }
        }
                    
    }
}
