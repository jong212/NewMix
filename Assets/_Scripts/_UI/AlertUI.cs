using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlertUI : MonoBehaviour
{
    [SerializeField] private Image _alertTitleImage;
    [SerializeField] private Text _alertTitleText;
    [SerializeField] private Button _customButton;
    public delegate void ClickConfirmButton();

    // 여기서는 매게변수를 두개만 쓰지만 확인 버튼 클릭했을 때 콜백처리 해야할 것 있으면 함수를 콜백으로 전달해서 실행시켜도 될 듯 
    public void OpenWarningUI(string titleText, string infoText)
    {
       OpenWarningUI(titleText, infoText, null);
    }

 
    public void OpenWarningUI(string titleText, string infoText, ClickConfirmButton clickConfirmButton)
    {
        _alertTitleImage.color = new Color32(203, 88, 0, 255);
        OpenUI(titleText, infoText, clickConfirmButton);
    }
    private void OpenUI(string titleText, string infoText,ClickConfirmButton clickConfirmButton)
    {
        gameObject.SetActive(true);

        _alertTitleText.text = titleText;

        _customButton.onClick.RemoveAllListeners();
        _customButton.onClick.AddListener(() => {
            CloseUI();
            if (clickConfirmButton != null)
            {
                clickConfirmButton.Invoke();
            }
        });
    }
    private void CloseUI()
    {
        gameObject.SetActive(false);
    }
}
