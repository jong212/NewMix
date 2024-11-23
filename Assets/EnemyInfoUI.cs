using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyInfoUI : MonoBehaviour
{
    public Slider Slider;
    public Text Level;
    public Text HpPercent;

    private void Awake()
    {
        if(Slider == null || Level == null || HpPercent == null)
        {
            Debug.LogError("EnemyINfoUI noref");
        }
    }
}
