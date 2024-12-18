using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonExpHpMpContainer : MonoBehaviour
{
    private Enemy character;

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
       /* character = StaticManager.Instance.UniquePlayer;
        character.OnStatsChanged += UpdateStats;
        character.InitHpUpdate();*/
 
    } 
    private void OnDisable()
    {
/*       character.OnStatsChanged -= UpdateStats;
*/
    }
    private void UpdateStats()
    {
        /*_playerHp.fillAmount = (float)character.CurrentHp / character.FinalHP;
        Debug.Log(character.CurrentHp + "ddd" + character.FinalHP);*/
    }
}
