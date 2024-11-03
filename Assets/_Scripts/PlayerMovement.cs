// PlayerMovement.cs
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;             // 플레이어 이동 속도 (units per second)
    [SerializeField] private float rotationSpeed = 3600f;  // 플레이어 회전 속도 (degrees per second)

    private List<Node> path;                             // 현재 경로
    private int targetIndex;                             // 현재 목표 노드 인덱스
    private Grid grid;
    [SerializeField] Pathfinding pathfinding;
    [SerializeField] private SimpleKCC simpleKCC;         // Simple KCC 컴포넌트 참조

    // 이동 관련 변수
    private Vector3 currentWaypoint;
    private bool isFollowingPath = false;

    public override void Spawned()
    {
        // 그리드와 Pathfinding 컴포넌트 참조
        grid = FindObjectOfType<Grid>();
        // pathfinding = GetComponent<Pathfinding>();
        //simpleKCC = GetComponent<SimpleKCC>();           // Simple KCC 컴포넌트 참조

        if (simpleKCC == null)
        {
            Debug.LogError("Simple KCC 컴포넌트를 찾을 수 없습니다. 플레이어 오브젝트에 Simple KCC를 추가하세요.");
        }

        // 네트워크 권한이 있는 클라이언트에서만 경로 업데이트를 시작합니다.
        if (Object.HasInputAuthority)
        {
            // Pathfinding의 경로 업데이트를 위한 이벤트 구독
            if (pathfinding != null)
            {
                pathfinding.OnPathUpdated += OnPathUpdated;
            }
            else
            {
                Debug.LogError("Pathfinding 컴포넌트를 찾을 수 없습니다.");
            }
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        // 이벤트 구독 해제
        if (pathfinding != null)
        {
            pathfinding.OnPathUpdated -= OnPathUpdated;
        }
    }

    // Pathfinding에서 경로가 업데이트될 때 호출되는 메서드
    private void OnPathUpdated(List<Node> newPath)
    {
        if (newPath != null && newPath.Count > 0)
        {
            path = newPath;
            targetIndex = 0;
            currentWaypoint = path[targetIndex].worldPosition;
            isFollowingPath = true;
            Debug.Log("새 경로 설정됨. 웨이포인트 수: " + path.Count);
        }
        else
        {
            isFollowingPath = false;
            Debug.LogWarning("유효한 경로가 없습니다.");
        }
    }

    // FixedUpdateNetwork는 네트워크 틱마다 호출됩니다.
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority)
            return; // 입력 권한이 없는 클라이언트는 이동하지 않음

        if (isFollowingPath && path != null && path.Count > 0)
        {
            // 현재 웨이포인트에 도달했는지 확인
            Debug.Log("플레이어 몬스터 거리" + (Vector3.Distance(simpleKCC.transform.position, currentWaypoint)));
            if (Vector3.Distance(simpleKCC.transform.position, currentWaypoint) < 0.5f)
            {
                targetIndex++;
                if (targetIndex >= path.Count)
                {
                    Debug.Log("경로 이동 완료");
                    simpleKCC.Move(Vector3.zero);
                    isFollowingPath = false;
                    if (pathfinding != null)
                    {
                        pathfinding.target = null;
                    }
                    return;
                }
                currentWaypoint = path[targetIndex].worldPosition;
                Debug.Log("다음 웨이포인트로 이동: " + currentWaypoint);
            }

            // 이동 방향 계산 (y-성분 제거)
            Vector3 direction = currentWaypoint - simpleKCC.transform.position;
            direction = Vector3.ProjectOnPlane(direction, Vector3.up); // 수평 평면으로 투영
            if (direction.magnitude > 0f)
                direction = direction.normalized;
            else
                direction = Vector3.zero; // 이동 방향이 없을 경우

            // 이동 벡터 계산 (Simple KCC.Move는 속도 벡터를 필요로 함)
            Vector3 velocity = direction * speed; // Runner.DeltaTime을 곱지 않음

            //Debug.Log($"수평 이동 방향: {direction}, 속도: {velocity}");

            // Simple KCC.Move 호출 (단일 벡터)
            simpleKCC.Move(velocity);

            // 회전 로직: 입력 권한이 있는 클라이언트에서만 회전 처리
            if (direction != Vector3.zero)
            {
                // 목표 회전 각도 계산
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

                // 현재 회전 각도 추출
                float currentYAngle = simpleKCC.transform.eulerAngles.y;

                // 회전 각도 보간 (부드러운 회전)
                float newYAngle = Mathf.MoveTowardsAngle(currentYAngle, targetAngle, rotationSpeed * Runner.DeltaTime);

                // Simple KCC를 통한 회전 적용
                simpleKCC.SetLookRotation(0, newYAngle);

                // Debug.Log 회전 상태 확인
                //Debug.Log($"현재 회전 각도: {currentYAngle}, 목표 회전 각도: {targetAngle}, 새로운 회전 각도: {newYAngle}");
            }
        }
    }

    // Optional: Gizmos를 사용하여 이동 방향 시각화
    void OnDrawGizmos()
    {
        if (isFollowingPath && path != null && targetIndex < path.Count)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentWaypoint);

            // 이동 방향 표시
            Vector3 direction = currentWaypoint - transform.position;
            direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + direction * 2f);
        }
    }
}
