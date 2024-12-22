using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;

public class PlayerMovement : NetworkBehaviour
{
    //  SerialrizeField //
    [SerializeField] private float speed = 5f;            // 플레이어 이동 속도 (units per second)
    [SerializeField] private float rotationSpeed = 3600f; // 플레이어 회전 속도 (degrees per second)
    [SerializeField] private SimpleKCC simpleKCC;         // Simple KCC 컴포넌트 참조
    [SerializeField] private Character character;         // Character 클래스 참조 (인스펙터에서 할당)
    [SerializeField] Pathfinding pathfinding;

    //  Private Field //
    private Vector3 currentWaypoint;
    private Grid grid;


    //  Getter Setter TO DO 변수 용도 각각 메모하기
    public Pathfinding Pathfinding => pathfinding;
    public List<Node> path;                              

    public override void Spawned()
    {
        //  얼리리턴
        if (!Object.HasInputAuthority) return;

        //  NullCheck
        if (simpleKCC == null) simpleKCC = GetComponent<SimpleKCC>();
        if (grid == null) grid = FindObjectOfType<Grid>();
        if (character == null)   character = GetComponent<Character>();

        // Event Add
        if (Pathfinding != null) Pathfinding.OnPathUpdated += OnPathUpdated; // Pathfinding의 경로 업데이트를 위한 이벤트 구독
    }

    /// <summary>
    /// 이벤트 구독 해제
    /// </summary>
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (pathfinding != null) pathfinding.OnPathUpdated -= OnPathUpdated;
    }

    /// <summary>
    /// 플레이어와 도착지점이 멀 수록 아래 리스트의 개수(note)가 많아지고 가까워 질수록 적어지도록 다른 곳에서 처리를 해뒀다
    /// </summary>
    private void OnPathUpdated(List<Node> newPath)
    {
        if (newPath != null)
        {
            path = newPath; 
            currentWaypoint = path[0].worldPosition; 
        }
    }

    /// <summary>
    /// 조이스틱 이동이 아닌 Astar를 통해 몬스터에게 이동하는 로직이다 
    /// 특정 거리만큼 좁혀졌다면 공격하는 로직이다.
    /// </summary>
    public void Movement()
    {
        if (path != null && path.Count > 0)
        {
            Debug.Log("11111");
            character.currentState = Character.chrState.TargetMove;

            //최종 목적지 위치값을 path[path.Count-1].worldPosition 으로 구하고 플레이어의 현 위치를 빼면 거리가 나오는데 1 미만인 경우에는 공격로직 타도록했음
            if (Vector3.Distance(simpleKCC.transform.position, path[path.Count-1].worldPosition) < 1f) 
            {
                    simpleKCC.Move(Vector3.zero);
                  
                    if(!character.IsAttack)
                    {
                        character.PerformAttack();
                    }
                    return;
            }

            // 아래 코드는 다음과 같이 비유할 수 있다.
            // 강남역에 가기 위해 역삼 선릉 강남중 첫 정거장(currentWaypoint)인 역삼에 도착하면(0.1f)
            // 그 다음 목적지는 선릉이 되는데 그 역삼에서 선릉으로 바꿔주는 로직이 아래와 같은 것이다.

            // 아래 로직을 않았을 때 작성하지 않고 Pahtfinder 스크립트의 Update문의 Pathfind 함수를 1초로 하면 플레이어가 currentWaypoint에 도착시 다음 도착지점이 있음에도 불구하고 도차간 지점에서 더이상 변경사항이 없기 때문에  제자리에서 도는 문제가 발생한다
            // 
            //Debug.Log(path.Count + "거리 개수");
            if (Vector3.Distance(simpleKCC.transform.position, currentWaypoint) < 0.1f)
            {
                int tempIdx = 0;
                foreach(Node worldPosition in path)
                {
                    if(worldPosition.worldPosition == currentWaypoint)
                    {
                        if (tempIdx + 1 >= path.Count)
                        {
                            Debug.Log("다음 인덱스가 범위를 초과합니다. 루프를 종료합니다.");
                            break; // 범위를 초과하므로 루프 종료
                        }
                        currentWaypoint = path[tempIdx + 1].worldPosition;
                        break; // 웨이포인트를 찾았으므로 루프 종료
                    }
                    tempIdx++;
                }
            }
            Vector3 direction = currentWaypoint - simpleKCC.transform.position; // 이동 방향 계산 (y-성분 제거)
            direction = Vector3.ProjectOnPlane(direction, Vector3.up);          // 수평 평면으로 투영

            if (direction.magnitude > 0f)
                direction = direction.normalized;
            else
                direction = Vector3.zero;                                       // 이동 방향이 없을 경우
           
            Vector3 velocity = direction * speed;   // Runner.DeltaTime을 곱지 않음  // 이동 벡터 계산 (Simple KCC.Move는 속도 벡터를 필요로 함)
            simpleKCC.Move(velocity);               // Simple KCC.Move 호출 (단일 벡터)
            if (direction != Vector3.zero)          // 회전 로직: 입력 권한이 있는 클라이언트에서만 회전 처리
            { 
                // 목표 회전 각도 계산
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

                // 현재 회전 각도 추출
                float currentYAngle = simpleKCC.transform.eulerAngles.y;

                // 회전 각도 보간 (부드러운 회전)
                float newYAngle = Mathf.MoveTowardsAngle(currentYAngle, targetAngle, rotationSpeed * Runner.DeltaTime);

                // Simple KCC를 통한 회전 적용
                simpleKCC.SetLookRotation(0, newYAngle);
            }
        } else
        {
            Debug.Log("2222222");
        }
    }

}
