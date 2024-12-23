using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpHpMpContainer : MonoBehaviour
{
    private Character character;

    [Header("Player")]
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
        character = StaticManager.Instance.UniquePlayer;
        character.OnStatsChanged += UpdateStats;
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

    }
    private void UpdateStats()
    {
        _playerHp.fillAmount = (float)character.CurrentHp / character.FinalHP;
    }
}
