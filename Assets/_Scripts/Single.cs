using UnityEngine;

public class Single : MonoBehaviour
{
    public static Single instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 부모 오브젝트가 씬 전환 간 유지되도록 설정
        }
        else
        {
            Destroy(gameObject); // 이미 인스턴스가 존재하면 새로 생성된 오브젝트는 삭제
        }
    }
}
