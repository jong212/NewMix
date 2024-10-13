using UnityEngine;

public class Single : MonoBehaviour
{
    public static Single instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // �θ� ������Ʈ�� �� ��ȯ �� �����ǵ��� ����
        }
        else
        {
            Destroy(gameObject); // �̹� �ν��Ͻ��� �����ϸ� ���� ������ ������Ʈ�� ����
        }
    }
}
