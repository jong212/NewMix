using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEditor;

public class MyMonsterMovement : NetworkBehaviour
{
    //  SerialrizeField //
    [SerializeField] private float speed = 5f;            // 플레이어 이동 속도 (units per second)
    [SerializeField] private float rotationSpeed = 3600f; // 플레이어 회전 속도 (degrees per second)
    [SerializeField] private SimpleKCC simpleKCC;         // Simple KCC 컴포넌트 참조
    [SerializeField] private Mentity character;         // Character 클래스 참조 (인스펙터에서 할당)
    [SerializeField] MyMonsterPathfinding pathfinding;

    //  Private Field //
    private Vector3 currentWaypoint;
    private Grid grid;
    

    //  Getter Setter TO DO 변수 용도 각각 메모하기
    public MyMonsterPathfinding Pathfinding => pathfinding;
    public List<Node> path;                              

    public override void Spawned()
    {
        //  얼리리턴
        if (!Object.HasStateAuthority) return;

        //  NullCheck
        if (simpleKCC == null) simpleKCC = GetComponent<SimpleKCC>();
        if (grid == null) grid = FindObjectOfType<Grid>();
        if (character == null)   character = GetComponent<Mentity>();

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

    public bool CanMove;
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (Pathfinding.target != null)
        {
            Movement();
        } 
    }
    /// <summary>
    /// 조이스틱 이동이 아닌 Astar를 통해 몬스터에게 이동하는 로직이다 
    /// 특정 거리만큼 좁혀졌다면 공격하는 로직이다.
    /// </summary>
    public void Movement()
    {
        if (Pathfinding.target == null) return;

        if (path != null && path.Count > 0)
        {
            // 최종 목적지와의 거리 확인 (공격 로직)
            if (Vector3.Distance(simpleKCC.transform.position, path[path.Count - 1].worldPosition) < 3f)
            {
                simpleKCC.Move(Vector3.zero);

                if (character._mai.stateMachine.currentState != character._mai.idleState)
                {
                    character._mai.stateMachine.ChangeState(character._mai.idleState);
                }

                character.PerformAttack();
                return;
            }

            if (character._mai.stateMachine.currentState != character._mai.moveState)
            {
                character._mai.stateMachine.ChangeState(character._mai.moveState);
            }

            // 웨이포인트 도착 시 다음 웨이포인트로 전환
            if (Vector3.Distance(simpleKCC.transform.position, currentWaypoint) < 0.1f)
            {
                int tempIdx = 0;
                foreach (Node worldPosition in path)
                {
                    if (worldPosition.worldPosition == currentWaypoint)
                    {
                        if (tempIdx + 1 >= path.Count)
                        {
                            break; // 범위를 초과하므로 루프 종료
                        }

                        currentWaypoint = path[tempIdx + 1].worldPosition;
                        break; // 웨이포인트를 찾았으므로 루프 종료
                    }
                    tempIdx++;
                }
            }

            // 이동 방향 계산
            Vector3 direction = currentWaypoint - simpleKCC.transform.position;
            direction = Vector3.ProjectOnPlane(direction, Vector3.up); // y-성분 제거 (수평 평면 투영)

            if (direction.magnitude > 0f)
            {
                direction = direction.normalized;
            }
            else
            {
                direction = Vector3.zero;
            }

            // 부드러운 속도 보간
            Vector3 currentVelocity = simpleKCC.RealVelocity; // SimpleKCC에서 현재 속도를 가져온다고 가정
            Vector3 targetVelocity = direction * StaticManager.Instance.UniquePlayer.FinalMoveSpeed;

            // Lerp로 부드럽게 속도 변경
            Vector3 smoothedVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.deltaTime * 5f); // 5f는 감속 강도

            // Simple KCC 이동 호출
            simpleKCC.Move(smoothedVelocity);

            // 회전 처리 (부드러운 회전)
            if (direction != Vector3.zero)
            {
                // 현재 방향과 목표 방향 계산
                Quaternion currentRotation = simpleKCC.transform.rotation;
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

                // 부드럽게 회전 (Slerp 사용)
                Quaternion smoothedRotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * 5f);

                // 회전 적용
                simpleKCC.SetLookRotation(smoothedRotation);
            }

        }
    }

}
