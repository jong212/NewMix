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
        var bro = Backend.Initialize(); // �ڳ� �ʱ�ȭ

        // �ڳ� �ʱ�ȭ�� ���� ���䰪

        if (bro.IsSuccess())
        {
            StartGoogleLogin();
            Debug.Log("�ʱ�ȭ ���� : " + bro.StatusCode);

        }
        else
        {
            Debug.LogError("�ʱ�ȭ ���� : " + bro);
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

        /*Debug.Log("���� ��ū : " + token);*/
        var bro = Backend.BMember.AuthorizeFederation(token, FederationType.Google);

        Debug.Log("�䵥���̼� �α��� ��� : " + bro);

        if (bro.StatusCode == 200 || bro.StatusCode == 201) //200 : ���� ȸ�� �α��� ����, 201 �ű� ����� ȸ������ �� �α��� ����
        {
            var nickData = Backend.BMember.GetUserInfo();
            LitJson.JsonData userInfoJson = nickData.GetReturnValuetoJSON()["row"];
            string nick = userInfoJson["nickname"]?.ToString();

            // �г����� ������� > �г��� ���� UI ����
            if (string.IsNullOrEmpty(nick))
            {                
                StaticManager.UI.OpenUI<BackEndSetName>("Prefabs/LoginScene/UI", LoginUICanvas.transform);
            }
            else // TO DO �г��� ���� �Ǿ����� > ���� ó�� ���� �ۼ� �ʿ�
            {
                Debug.Log($"�г���: {nick}");
            }
        }

    }

}
