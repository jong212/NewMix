using BackEnd;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;

// OnClickCreateNicknameButton > BaseUI, protected, ShowAlertUI() > StaticManager > UIManager > AlertUI > AlertUI�� OpenWarningUI ����
public class BackEndSetName : BaseUI
{
    [SerializeField] InputField _nicknameCreateInput;
    [SerializeField] Button _nicknameCreateButton ;
    private void Awake()
    {
        _nicknameCreateButton.onClick.AddListener(OnClickCreateNicknameButton);
    }

    // �г��� ���� ��ư Ŭ�� �� ȣ�� �Ǵ� �Լ�
    void OnClickCreateNicknameButton()
    {
       // �����?
       var nickname = _nicknameCreateInput.text;
        if (string.IsNullOrEmpty(nickname))
        {
            ShowAlertUI("�ȳ�","�г����� ����ֽ��ϴ�.");
            return;
        } 
        else
        {
            // �ߺ��ΰ�?
            Backend.BMember.CheckNicknameDuplication(nickname, (callback) =>
            {
                if(callback.StatusCode == 204)
                {
                    // ��� �� ������? Open Confirm UI
                    ShowConfirmUI("�ȳ�", "��� ������ ���̵��Դϴ�", null, null, () => SetNick(nickname));
                }
                else if (callback.StatusCode == 409)
                {
                    ShowAlertUI("�ȳ�", "�г��� �ߺ��Դϴ�.");

                }
            });
        }
    }

    void SetNick(string nickname)
    {
        Backend.BMember.CreateNickname(nickname, (callback) =>
        {
            Debug.Log("�г��� ���� �Ϸ�");
            int chrIndex = LoginSceneManager.Instance.Selecter.selectedCharacter;
            BackendGameData.Instance.GameDataInsert(chrIndex);
        });
    }
}
