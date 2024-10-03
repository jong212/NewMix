using UnityEngine;

public class IsometricCameraFollow : MonoBehaviour
{
    public Transform target;  // 플레이어(또는 카메라가 따라갈 대상)
    public Vector3 offset = new(0, 1.08f, 0);    // 카메라와 플레이어 사이의 거리

    void Start()
    {
        // 타겟이 지정되어 있을 때만 오프셋 계산
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        // 타겟이 있을 때만 카메라 위치를 갱신
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}
