using UnityEngine;

public class IsometricCameraFollow : MonoBehaviour
{
    public Transform target;  // �÷��̾�(�Ǵ� ī�޶� ���� ���)
    public Vector3 offset = new(0, 1.08f, 0);    // ī�޶�� �÷��̾� ������ �Ÿ�

    void Start()
    {
        // Ÿ���� �����Ǿ� ���� ���� ������ ���
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        // Ÿ���� ���� ���� ī�޶� ��ġ�� ����
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}
