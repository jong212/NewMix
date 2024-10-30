using BackEnd;
using LitJson;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.TextCore.Text;


public class StaticManager : MonoBehaviour
{
    // 싱글톤
    public static StaticManager Instance { get; private set; }

    // 캐싱용 - 로컬에 다운 받아진 사용가능한 차트이름들
    private List<string> _getCharLocalListname = new List<string>();
    public List<string>  GetCharLocalListname {
        get => _getCharLocalListname;
        set => _getCharLocalListname = value;
    }
    
    // 캐싱용 - 기본 캐릭터 어드레서블 차트
    private List<CharacterSrcChart> _characterList = new List<CharacterSrcChart>();
    public List<CharacterSrcChart> CharacterList
    {
        get => _characterList;
        set => _characterList = value;
    }

    // 캐싱용 - 테스트
    public List<Test> _test = new List<Test>();
    public List<Test> Test 
    { 
        get => _test; 
        set => _test = value; 
    }

    public static UIManager UI { get; private set; }

    void Awake()
    {
        Init();
    }
    private void Update()
    {
        if(CharacterList != null)
        {
            foreach(var c in CharacterList)
            {
                Debug.Log(c);
            }
        }
    }
    void Init()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        UI = GetComponentInChildren<UIManager>();

        UI.Init(); 
    }

    // 모바일에서 오브젝트 위치 확인용
    public void LogHierarchyPath(Transform transform)
    {
        if (transform == null)
        {
            Debug.LogError("Transform is null. Cannot log hierarchy path.");
            return;
        }

        string path = transform.name;
        Transform currentParent = transform.parent;

        // 부모를 따라 올라가며 전체 경로 생성
        while (currentParent != null)
        {
            path = currentParent.name + "/" + path;
            currentParent = currentParent.parent;
        }

        // 이름과 경로를 로그로 출력
        Debug.Log($"Object name: {transform.name}, Path: {path}");
    }

    #region 차트 데이터 캐싱용 비즈니스 로직
    public void InitSetting()
    {
        // 서버에서 데이터 새로 받아오기 위해 중복 초기화?
        BackendGameData.Instance.GetPlayerData();

        foreach (var chartName in GetCharLocalListname)
        {
            LoadChart(chartName);
        }

    }
    private void LoadChart (string chartName)
    {
        string chartDataString = Backend.Chart.GetLocalChartData(chartName);
        JsonData chartJson = JsonMapper.ToObject(chartDataString);
        chartJson = BackendReturnObject.Flatten(chartJson);

        switch (chartName) 
        {
            case nameof(CharacterSrcChart):
                foreach (JsonData row in chartJson["rows"])
                {
                    CharacterSrcChart classRef = new CharacterSrcChart(row);
                    CharacterList.Add(classRef);
                }
                break;
            case nameof(Test):
                foreach (JsonData row in chartJson["rows"])
                {
                    Test classRef = new Test(row);
                    Test.Add(classRef);
                }
                break;
        }
    }
    #endregion
    public void CashData()
    {
        UserData cashPlayerData = BackendGameData.userData;
    }
}
