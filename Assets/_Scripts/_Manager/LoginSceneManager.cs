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

        switch (bro.StatusCode)
        {
            case 200: Debug.Log("success"); StaticManager.UI.OpenUI<BackEndSetName>("Prefabs/LoginScene/UI", LoginUICanvas.transform);
                break;
            case 201: ;
                break;
        }
    }

}
