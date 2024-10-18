using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmUI : MonoBehaviour
{
    [SerializeField] private Image _headerContainer;
    [SerializeField] private Text _headerText;
    [SerializeField] private Text _infoText;
    [SerializeField] private Button _okBtn;
    [SerializeField] private Button _cancelBtn;
    public delegate void ClickConfirmOkButton();


    public void OpenConfirmUI(string titleText, string infoText, string okBtnText, string cancelBtnText, ClickConfirmOkButton okCallback)
    {
        _headerContainer.color = new Color32(203, 88, 0, 255);
        OpenUI(titleText, infoText, okBtnText, cancelBtnText, okCallback);
    }

    private void OpenUI(string titleText, string infoText, string okBtnText , string cancelBtnText, ClickConfirmOkButton okCallback)
    {
        gameObject.SetActive(true);

        _headerText.text = titleText;
        _infoText.text = infoText;

        if(okBtnText != null) 
        {
            _okBtn.GetComponentInChildren<Text>().text = okBtnText;            
        }

        if(cancelBtnText != null)
        {
            _cancelBtn.GetComponentInChildren<Text>().text = cancelBtnText;            
        }

        _okBtn.onClick.RemoveAllListeners();
        _okBtn.onClick.AddListener(() => {
            CloseUI();
            if (okCallback != null)
            {
                okCallback.Invoke();
            }
        });

        _cancelBtn.onClick.RemoveAllListeners();
        _cancelBtn.onClick.AddListener(() => {
            CloseUI();
        });

    }
    private void CloseUI()
    {
        gameObject.SetActive(false);
    }
}
