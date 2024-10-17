using BackEnd;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

// OnClickCreateNicknameButton > BaseUI, protected, ShowAlertUI() > StaticManager > UIManager > AlertUI > AlertUI의 OpenWarningUI 실행
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
            ShowAlertUI("닉네임이 비어있습니다.");
            return;
        } 
        else
        {
            Backend.BMember.CheckNicknameDuplication(nickname, (callback) =>
            {
                if(callback.StatusCode == 204)
                {
                    Debug.Log("해당 닉네임으로 수정 가능합니다");
                }
                else if (callback.StatusCode == 409)
                {
                    Debug.Log("중복입니다");

                }
            });
        }
    }
    void Start()
    {
        /*Backend.BMember.CreateNickname("thebackend", (callback) =>
        {
            // 이후 처리
        });*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
