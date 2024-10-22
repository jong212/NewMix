using BackEnd;
using LitJson;
using UnityEngine;

public class EnemyInfo
{
    public string Name { get; private set; }
    public int HP { get; private set; }
    public int Attack { get; private set; }

    public EnemyInfo(JsonData json)
    {
        Name = json["name"].ToString();
        HP = int.Parse(json["hp"].ToString());
        Attack = int.Parse(json["attack"].ToString());
    }
}

public class StaticManager : MonoBehaviour
{
    public static StaticManager Instance { get; private set; }

    public static UIManager UI { get; private set; }


    // 모든 씬에서 사용되는 기능들을 모아놓은 클래스.
    // 각씬메니저가 현재 쌘에 존재하는지 확인 후 생성한다.
    void Awake()
    {
        Init();
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

    public void InitSetting()
    {
        BackendGameData.Instance.GetPlayerData();   // 서버에서 데이터 새로 받아오기
        JsonData charJson = JsonMapper.ToObject(Backend.Chart.GetLocalChartData("CharacterSrcChart"));
        charJson = BackendReturnObject.Flatten(charJson);
    }

    public void CashData()
    {
        UserData cashPlayerData = BackendGameData.userData;
    }
}
