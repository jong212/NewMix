using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyInfoUI : MonoBehaviour
{
    public Slider Slider;
    public Text Level;
    [SerializeField] private Text _hpPercent;
    public string HpPercentText
    {
        get => _hpPercent != null ? _hpPercent.text : string.Empty;
        set
        {
            if (_hpPercent != null)
            {
                _hpPercent.text = value + "%";

                if (int.TryParse(value, out int hpValue))
                {
                    if (hpValue <= 0)
                    {
                        this.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    public Transform ObjRef;
    private void Awake()
    {
        if(Slider == null || Level == null  )
        {
            Debug.LogError("EnemyINfoUI noref");
        }
    }
    
}
