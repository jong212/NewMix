using BackEnd;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LoginSceneManager : MonoBehaviour
{
    private static LoginSceneManager _instance;

    public static LoginSceneManager Instance
    {
        get
        {
            return _instance;
        }
    }

    [SerializeField] private Canvas _loginUICanvas;
    public Canvas LoginUICanvas
    {
        get
        {
            return _loginUICanvas;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        if (FindObjectOfType(typeof(StaticManager)) == null) { 
            var obj = Resources.Load<GameObject>("Prefabs/StaticManager");
            Instantiate(obj);

        }
        var bro = Backend.Initialize(); // 뒤끝 초기화

        // 뒤끝 초기화에 대한 응답값

        if (bro.IsSuccess())
        {
            StartGoogleLogin();
            Debug.Log("초기화 성공 : " + bro.StatusCode);

        }
        else
        {
            Debug.LogError("초기화 실패 : " + bro);
        }


    }
    public void StartGoogleLogin()
    {
        TheBackend.ToolKit.GoogleLogin.Android.GoogleLogin(true, GoogleLoginCallback);
    }

    private void GoogleLoginCallback(bool isSuccess, string errorMessage, string token)
    {
        if (isSuccess == false)
        {
            Debug.LogError(errorMessage);
            return;
        }

        /*Debug.Log("구글 토큰 : " + token);*/
        var bro = Backend.BMember.AuthorizeFederation(token, FederationType.Google);

        Debug.Log("페데레이션 로그인 결과 : " + bro);

        if (bro.StatusCode == 200 || bro.StatusCode == 201) //200 : 기존 회원 로그인 성공, 201 신규 사용자 회원가입 및 로그인 성공
        {
            var nickData = Backend.BMember.GetUserInfo();
            LitJson.JsonData userInfoJson = nickData.GetReturnValuetoJSON()["row"];
            string nick = userInfoJson["nickname"]?.ToString();

            // 닉네임이 비어있음 > 닉네임 설정 UI 오픈
            if (string.IsNullOrEmpty(nick))
            {                
                StaticManager.UI.OpenUI<BackEndSetName>("Prefabs/LoginScene/UI", LoginUICanvas.transform);
            }
            else // TO DO 닉네임 설정 되어있음 > 이후 처리 로직 작성 필요
            {
                Debug.Log($"닉네임: {nick}");
            }
        }

    }

}
