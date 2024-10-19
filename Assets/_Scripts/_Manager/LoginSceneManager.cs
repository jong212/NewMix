using BackEnd;
using LitJson;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

 

public class LoginSceneManager : MonoBehaviour
{
    private static LoginSceneManager _instance;
    private GameObject playerPrefab;
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
        //SceneManager.LoadScene("Preloader"); //�α��� �����ϰ� ���Ӿ����� �����ϴ� �׽�Ʈ �ϱ� ���� �ӽ� �ּ� Test���� �� ���� �� �ּ�ó��

        if (FindObjectOfType(typeof(StaticManager)) == null) { 
            var obj = Resources.Load<GameObject>("Prefabs/StaticManager");
            Instantiate(obj);

        }
        var bro = Backend.Initialize(); // �ڳ� �ʱ�ȭ

        // �ڳ� �ʱ�ȭ�� ���� ���䰪

        if (bro.IsSuccess())
        {
            StartGoogleLogin();
            Debug.Log("[1] �ʱ�ȭ ���� : " + bro.StatusCode);

        }
        else
        {
            Debug.LogError("�ʱ�ȭ ���� : " + bro);
        } 
    }


    private void OnEnable() // awake ������ �ٲ�� �ҵ�
    {
        if (AddressableManager.instance == null)
        {
            Debug.LogError("AddressableManager instance is not initialized.");
        }
        AddressableManager.instance.LoadPrefabsWithLabel("Init", () =>
        {
            playerPrefab = AddressableManager.instance.GetPrefab("Stickman_White");

            if (playerPrefab != null)
            {
                var instantiatedPrefab = Instantiate(playerPrefab); // ������ �ν��Ͻ�ȭ
                var anim = instantiatedPrefab.GetComponent<Animator>();

                if (anim == null)
                {
                    Debug.Log("Animator is null on the instantiated prefab.");
                }
                else
                {
                    anim.SetFloat("Movement", 1);
                    Debug.Log("dd");
                }
            }
            else
            {
                Debug.LogError("Failed to load playerPrefab: Stickman_White");
            }
        });
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

        var bro = Backend.BMember.AuthorizeFederation(token, FederationType.Google);
        Debug.Log("[2] ���� �䵥���̼� �α��� ��� : " + bro);

        StartCoroutine(CheckChartUpdate());



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
              SceneManager.LoadScene("Preloader");

            }
        }

    }
    public class ChartInfo
    {
        public string chartName;
        public string chartFileId;
        public string updateDate;

        public ChartInfo(JsonData json)
        {
            chartName = json["chartName"].ToString();
            chartFileId = json["chartFileId"].ToString();
            updateDate = json["updateDate"].ToString();
        }
    }

    IEnumerator CheckChartUpdate()
    {
        // ��Ʈ �Ŵ��� ���� ���� 
        Debug.Log("CheckChart");
        var bro = Backend.Chart.GetChartListByFolder(2056);

        // �ش� �������� chartManager ��Ʈ �ϳ��� ������ ���̹Ƿ� 0���� �����մϴ�.
        string chartManagerFileId = bro.FlattenRows()[0]["selectedChartFileId"].ToString(); // CSV ���� ���ε� �� �� �ο� �� ���� ���� ID ���� ������ ex 145150
        string chartManagerName = bro.FlattenRows()[0]["chartName"].ToString();
        // �������� ChartManager ��Ʈ�� �ҷ��ɴϴ�. ��⿡ ���������� �ʽ��ϴ�.
        var serverChartBro = Backend.Chart.GetChartContents(chartManagerFileId);

        // �������� �ҷ����� ���� ��쿡�� ������ ���� ������ ���� ������ �����մϴ�.
        if (serverChartBro.IsSuccess() == false)
        {
            Debug.Log("CheckChartStop");

            yield break;
        }

        // �������� �ҷ��� ChartManager�� �𸶼��Ͽ� JsonData ���·� ĳ���մϴ�.
        JsonData newChartManagerJson = serverChartBro.FlattenRows();

        // ��Ʈ �̸����� �����͸� �˻��� ���̱� ������ Dictnary�� �����մϴ�.
        // �ش� Dictnary�� �ֽ� �������� ������Ʈ�� ��Ʈ ����Ʈ�� ���˴ϴ�.(�ֽ� �����̶�� �ش� ����Ʈ���� ����)
        Dictionary<string, ChartInfo> chartInfoDic = new Dictionary<string, ChartInfo>();

        // csv �� �� = charinfojson
        foreach (JsonData chartInfoJson in newChartManagerJson)
        {
            Debug.Log(chartInfoJson + "2222");
            ChartInfo chartInfo = new ChartInfo(chartInfoJson);
            chartInfoDic.Add(chartInfo.chartName, chartInfo);
        }

        // ��⿡ ����� chartManager ��Ʈ�� �ҷ��ɴϴ�.
        string deviceChartManagerString = Backend.Chart.GetLocalChartData(chartManagerName);

        // ��⿡�� string ���·� ������ �Ǹ�, ����Ǿ����� ���� ��� string.Empty�� ��ȯ�˴ϴ�.
        if (string.IsNullOrEmpty(deviceChartManagerString) == false)
        {
            // ��⿡ ����� chartManager ��Ʈ�� �����Ѵٸ�

            // ��⿡ ����� string������ chartManager�� Json ���·� ����
            JsonData deviceChartManagerJson = JsonMapper.ToObject(deviceChartManagerString);
            deviceChartManagerJson = BackendReturnObject.Flatten(deviceChartManagerJson);

            // ��⿡ ����� chartManager ��Ʈ �� ��Ʈ���� �������� �ҷ��� �����Ϳ� �����մϴ�.
            foreach (JsonData deviceChartJson in deviceChartManagerJson["rows"])
            {
                ChartInfo deviceChartInfo = new ChartInfo(deviceChartJson);

                // �̹� ��⿡ ����Ǿ� �ִ� ��Ʈ�� �ִ��� Ȯ���մϴ�.
                if (chartInfoDic.ContainsKey(deviceChartInfo.chartName))
                {

                    // ��⿡ ����Ǿ� �ִ� ��Ʈ�� ���� ��¥(updateDate)�� ��ġ�ϴ��� Ȯ���մϴ�.
                    if (chartInfoDic[deviceChartInfo.chartName].updateDate == deviceChartInfo.updateDate)
                    {
                        // ������¥���� ��ġ�� ���, ��ٿ�ε� ����Ʈ(chartInfoDic)���� �����մϴ�.
                        chartInfoDic.Remove(deviceChartInfo.chartName);
                    }
                }
            }
        }

        // ��ٿ�ε��� ��Ʈ ����Ʈ���� ��Ʈ�� �ϳ��� �����ϴ��� Ȯ���մϴ�.
        if (chartInfoDic.Count > 0)
        {

            // ��Ʈ�� ��ٿ�ε��Ͽ� ��⿡ �����ϴ�.
            foreach (var downloadChartInfo in chartInfoDic)
            {
                Debug.Log(downloadChartInfo.Value.chartName + "�� ���ο� �������� �ٿ�޽��ϴ�.");
                Backend.Chart.GetOneChartAndSave(downloadChartInfo.Value.chartFileId, downloadChartInfo.Value.chartName);
            }

            // chartManager ��Ʈ�� �ֽ�ȭ�մϴ�.
            Backend.Chart.GetOneChartAndSave(chartManagerFileId, chartManagerName);
        }
        else
        {
            Debug.Log("������Ʈ�� ������ �������� �ʽ��ϴ�.");
        }

    }
}
