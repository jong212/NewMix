using BackEnd;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
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

    void OnClickCreateNicknameButton()
    {
       var nickname = _nicknameCreateInput.text;
        if (string.IsNullOrEmpty(nickname))
        {
            ShowAlertUI("�г����� ����ֽ��ϴ�.");
            return;
        } 
        else
        {
            Backend.BMember.CheckNicknameDuplication(nickname, (callback) =>
            {
                if(callback.StatusCode == 204)
                {
                    Debug.Log("�ش� �г������� ���� �����մϴ�");
                }
                else if (callback.StatusCode == 409)
                {
                    Debug.Log("�ߺ��Դϴ�");

                }
            });
        }
    }
    void Start()
    {
        /*Backend.BMember.CreateNickname("thebackend", (callback) =>
        {
            // ���� ó��
        });*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
