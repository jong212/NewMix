using BackEnd;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    // 닉네임 설정 버튼 클릭 시 호출 되는 함수
    void OnClickCreateNicknameButton()
    {
       // 비었나?
       var nickname = _nicknameCreateInput.text;
        if (string.IsNullOrEmpty(nickname))
        {
            ShowAlertUI("안내","닉네임이 비어있습니다.");
            return;
        } 
        else
        {
            // 중복인가?
            Backend.BMember.CheckNicknameDuplication(nickname, (callback) =>
            {
                if(callback.StatusCode == 204)
                {
                    // 사용 할 것인지? Open Confirm UI
                    ShowConfirmUI("안내", "사용 가능한 아이디입니다", null, null, () => SetNick(nickname));
                }
                else if (callback.StatusCode == 409)
                {
                    ShowAlertUI("안내", "닉네임 중복입니다.");

                }
            });
        }
    }

    void SetNick(string nickname)
    {
        Backend.BMember.CreateNickname(nickname, (callback) =>
        {
            Debug.Log("닉네임 설정 완료");
            int chrIndex = LoginSceneManager.Instance.Selecter.selectedCharacter;
            BackendGameData.Instance.GameDataInsert(chrIndex);
        });
    }
}
