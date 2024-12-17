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
        character = StaticManager.Instance.UniquePlayer;
        character.OnStatsChanged += UpdateStats;
        character.InitHpUpdate();
        /*StartCoroutine(getPlayer());*/

    }

    /*private IEnumerator getPlayer()
    {
        yield return null;
        
       
    
    }*/
    private void Update()
    {
        Debug.Log(character);

    }
    private void OnDisable()
    {
       character.OnStatsChanged -= UpdateStats;

    }
    private void UpdateStats()
    {
        _playerHp.fillAmount = (float)character.CurrentHp / character.FinalHP;
        Debug.Log(character.CurrentHp + "ddd" + character.FinalHP);
    }
}
