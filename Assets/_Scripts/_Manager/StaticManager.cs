using BackEnd;
using LitJson;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.TextCore.Text;


public class StaticManager : MonoBehaviour
{
    public static StaticManager Instance { get; private set; }      // 싱글톤
    public static UIManager UI { get; private set; }                // 인스펙터 참조하기 위해 public
    public static DataSetManager DataSetManager { get; private set; }                // 인스펙터 참조하기 위해 public


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
        /*DataSetManager = GetComponentInChildren<DataSetManager>(); 필요할 때 사용 아직 스태틱 매니저에서는 뭐 처리할 게 없어 보임*/
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
   
    public void CashData()
    {
        UserData cashPlayerData = BackendGameData.Instance.userData;
    }


}
