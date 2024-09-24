using UnityEngine;
using Fusion;

public class NetworkMovingPlatform : NetworkBehaviour
{
    public float speed = 2f;
    public float range = 3f;

    private Vector3 startPosition;
    private bool movingRight = true;

    public override void Spawned()
    {
        startPosition = transform.position;
    }

    public override void FixedUpdateNetwork()
    {
        // �÷����� ��ġ�� ��Ʈ��ũ���� ����ȭ
        if (Object.HasStateAuthority) // ���� �Ǵ� �� ������ ���� Ŭ���̾�Ʈ������ �̵� ó��
        {
            if (movingRight)
            {
                transform.position = Vector3.MoveTowards(transform.position, startPosition + Vector3.right * range, speed * Runner.DeltaTime);
                if (transform.position == startPosition + Vector3.right * range)
                    movingRight = false;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, startPosition - Vector3.right * range, speed * Runner.DeltaTime);
                if (transform.position == startPosition - Vector3.right * range)
                    movingRight = true;
            }
        }
    }
}
