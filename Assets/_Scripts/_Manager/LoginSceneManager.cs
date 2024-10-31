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
    public static LoginSceneManager Instance
    {
        get
        {
            return _instance;
        }
    }

    [SerializeField] private Canvas _loginUICanvas;
    [SerializeField] private CharacterSelection _selector;
    public CharacterSelection Selecter
    {
        get
        {
            return _selector;
        }

    }

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
        //SceneManager.LoadScene("Preloader"); //로그인 생략하고 게임씬으로 입장하는 테스트 하기 위한 임시 주석 Test종료 후 삭제 및 주석처리

        if (FindObjectOfType(typeof(StaticManager)) == null)
        {
            var obj = Resources.Load<GameObject>("Prefabs/StaticManager");
            Instantiate(obj);

        }
        var bro = Backend.Initialize(); // 뒤끝 초기화

        // 뒤끝 초기화에 대한 응답값

        if (bro.IsSuccess())
        {
            /* ================================================================================
             * StartGoogleLogin(); // PC 테스트는 CustomLogin 함수 사용하고 모바일은 StartGoogleLogin
             * ================================================================================*/
            CustomLogin();
            Debug.Log("초기화 성공 : " + bro.StatusCode);
        }
        else
        {
            Debug.LogError("초기화 실패 : " + bro);
        }
    }

    public void CustomLogin()
    {
        string id = "test123";                                           // 테스트용
        string password = "123";                                         // 테스트용

        var bro = Backend.BMember.CustomLogin(id, password);
        StartCoroutine(ServerCharLoad());

        if (bro.StatusCode == 200 || bro.StatusCode == 201)              // 200: 기존 회원, 201: 신규 사용자 회원가입 및 로그인 성공
        {
            var nickData = Backend.BMember.GetUserInfo();
            LitJson.JsonData userInfoJson = nickData.GetReturnValuetoJSON()["row"];
            string nick = userInfoJson["nickname"]?.ToString();
            BackendGameData.Instance.SetNickname(nick);                  // 캐싱   

                                    
            if (string.IsNullOrEmpty(nick))                              // 닉네임이 비어있음 > 닉네임 설정 UI 오픈
            {
                StaticManager.UI.CommonOpen(UIType.BackEndName, LoginUICanvas.transform, true);
                Selecter.gameObject.SetActive(true);
            }
            else                                                         // TO DO 닉네임 설정 되어있음 > 이후 처리 로직 작성 필요
            {
                SetWaitRoom();
            }
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

        var bro = Backend.BMember.AuthorizeFederation(token, FederationType.Google);
        Debug.Log("구글 페데레이션 로그인 결과 : " + bro);

        StartCoroutine(ServerCharLoad());                                              // 다운로드 차트

        if (bro.StatusCode == 200 || bro.StatusCode == 201)                              // 200: 기존 회원, 201: 신규 사용자 회원가입 및 로그인 성공
        {
            var nickData = Backend.BMember.GetUserInfo();
            LitJson.JsonData userInfoJson = nickData.GetReturnValuetoJSON()["row"];
            string nick = userInfoJson["nickname"]?.ToString();
            BackendGameData.Instance.SetNickname(nick);
           
            if (string.IsNullOrEmpty(nick))                 // 닉네임이 비어있음 > 닉네임 설정 UI 오픈
            {
                StaticManager.UI.CommonOpen(UIType.BackEndName, LoginUICanvas.transform, true);                
                Selecter.gameObject.SetActive(true);
            }
            else // TO DO 닉네임 설정 되어있음 > 이후 처리 로직 작성 필요
            {
                SetWaitRoom();
            }
        }

    }

    #region # 서버 차트 비교 후 로드
    IEnumerator ServerCharLoad()
    {
        Debug.Log("[3. 차트 매니저 접근]");
        
        var bro = Backend.Chart.GetChartListByFolder(2056);                                 // 차트 매니저 폴더 접근 
        string chartManagerFileId = bro.FlattenRows()[0]["selectedChartFileId"].ToString(); // CSV 파일 업로드 할 때 부여 된 고유 파일 ID 값을 가져옴 ex 145150, // 해당 폴더에는 chartManager 차트 하나만 존재할 것이므로 0으로 접근합니다.
        string chartManagerName = bro.FlattenRows()[0]["chartName"].ToString();
        var serverChartBro = Backend.Chart.GetChartContents(chartManagerFileId);            // 서버에서 ChartManager 차트를 불러옵니다. 기기에 저장하지는 않습니다.
        if (serverChartBro.IsSuccess() == false)                                            // 서버에서 불러오지 못할 경우에는 데이터 꼬임 방지를 위해 진행을 중지합니다.
        {
            Debug.Log("CheckChartStop");
            yield break;
        }

        Debug.Log("[3-1. 로컬, 서버 차트 비교 하기 위한 과정]");
        JsonData newChartManagerJson = serverChartBro.FlattenRows();                        // 서버에서 불러온 ChartManager을 언마샬하여 JsonData 형태로 캐싱합니다.
        Dictionary<string, ChartInfo> chartInfoDic = new Dictionary<string, ChartInfo>();   // 차트 이름으로 데이터를 검색할 것이기 때문에 Dictnary로 생성합니다, // 해당 Dictnary는 최신 버전으로 업데이트할 차트 리스트로 사용됩니다.(최신 버전이라면 해당 리스트에서 제외)

        
        foreach (JsonData chartInfoJson in newChartManagerJson)                             // csv 한 줄 = charinfojson
        {
            ChartInfo chartInfo = new ChartInfo(chartInfoJson);
            chartInfoDic.Add(chartInfo.chartName, chartInfo);
            BackendGameData.Instance.GetCharLocalListname.Add(chartInfo.chartName);
        }
        string deviceChartManagerString = Backend.Chart.GetLocalChartData(chartManagerName);// 기기에 저장된 chartManager 차트를 불러옵니다.
        if (string.IsNullOrEmpty(deviceChartManagerString) == false)                        // 기기에는 string 형태로 저장이 되며, 저장되어있지 않을 경우 string.Empty가 반환됩니다.
        {
            
            
            JsonData deviceChartManagerJson = JsonMapper.ToObject(deviceChartManagerString);// 기기에 저장된 chartManager 차트가 존재한다면 // 기기에 저장된 string형태의 chartManager를 Json 형태로 변경
            deviceChartManagerJson = BackendReturnObject.Flatten(deviceChartManagerJson);

            Debug.Log("[3-1. 로컬, 서버 차트 비교]");
            foreach (JsonData deviceChartJson in deviceChartManagerJson["rows"])            // 기기에 저장된 chartManager 차트 속 차트들을 서버에서 불러온 데이터와 대조합니다.
            {
                ChartInfo deviceChartInfo = new ChartInfo(deviceChartJson);
                if (chartInfoDic.ContainsKey(deviceChartInfo.chartName))                    // 이미 기기에 저장되어 있는 차트가 있는지 확인합니다.
                {
                    if (chartInfoDic[deviceChartInfo.chartName].updateDate == deviceChartInfo.updateDate)// 기기에 저장되어 있는 차트의 수정 날짜(updateDate)가 일치하는지 확인합니다.
                    {
                        chartInfoDic.Remove(deviceChartInfo.chartName);                     // 수정날짜까지 일치할 경우, 재다운로드 리스트(chartInfoDic)에서 제외합니다.
                    }
                }
            }
        }

        if (chartInfoDic.Count > 0)                                                         // 재다운로드할 차트 리스트에서 차트가 하나라도 존재하는지 확인합니다.
        {
            foreach (var downloadChartInfo in chartInfoDic)                                 // 차트를 재다운로드하여 기기에 덮어씌웁니다.
            {
                Debug.Log("[3-2 : "+downloadChartInfo.Value.chartName + "을 새로운 버전으로 다운받습니다.]");
                Backend.Chart.GetOneChartAndSave(downloadChartInfo.Value.chartFileId, downloadChartInfo.Value.chartName);
            }
            Backend.Chart.GetOneChartAndSave(chartManagerFileId, chartManagerName);         // chartManager 차트를 최신화합니다.(로컬저장)
        }
        else
        {
            Debug.Log("[3-2 : 업데이트할 내역이 존재하지 않습니다.]");
        }

    }
    #endregion

    #region # SetWaitRoom
    public void SetWaitRoom()
    {
        if(Selecter != null && Selecter.gameObject.activeSelf == true)
        {
            Selecter.gameObject.SetActive(false);
        }
         
        BackendGameData.Instance.GetPlayerData();
        GameObject infoUI = StaticManager.UI.CommonOpen(UIType.CharaterUI, LoginUICanvas.transform,false,true);
        PlayerWaitRoomInfo Roominfo = infoUI.GetComponent<PlayerWaitRoomInfo>();
        Roominfo.NickNameLayout.text = BackendGameData.Instance.NickName;
        
        UserData data = BackendGameData.Instance.userData;
        Roominfo.LvLayout.text = data.level.ToString();
        Roominfo.AtkLayout.text = data.atk.ToString();
        Roominfo.HpLayout.text = data.hp.ToString();
        Roominfo.MissLayout.text = data.miss.ToString();
        Roominfo.LastLocationLayout.text = data.lastMap.ToString();
    }
    #endregion

    // TEST 후 UPDATE 제거

    private void Update()
    {
        if (BackendGameData.Instance.CharacterList != null)
            foreach (var character in BackendGameData.Instance.CharacterList)
            {
                    Debug.Log($"Character ID: {character.charId}, Label Name: {character.labName}, Prefab Name: {character.prefName}");
            }
        var userData = BackendGameData.Instance.userData;
        var properties = userData.GetType().GetProperties(); // 모든 프로퍼티 가져오기

        foreach (var prop in properties)
        {
            var propName = prop.Name;                          // 프로퍼티 이름
            var propValue = prop.GetValue(userData);           // 프로퍼티 값
            Debug.Log($"{propName}: {propValue}");
        }

    }
}
