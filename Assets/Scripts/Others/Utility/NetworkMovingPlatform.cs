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
        // 플랫폼의 위치를 네트워크에서 동기화
        if (Object.HasStateAuthority) // 서버 또는 주 권한을 가진 클라이언트에서만 이동 처리
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
