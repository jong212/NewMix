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

    // ���⼭�� �ŰԺ����� �ΰ��� ������ Ȯ�� ��ư Ŭ������ �� �ݹ�ó�� �ؾ��� �� ������ �Լ��� �ݹ����� �����ؼ� ������ѵ� �� �� 
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
