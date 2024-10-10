using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 여기서부터 업데이트가 시작 됨 > 여기 업데이트 치고 > 현재 상태 업데이트 같이 치고 > 그래서 update, fixedupdae 추가했음 
public class Enemy : Entity
{
    [SerializeField] protected LayerMask ObstacleLayer;
    [SerializeField] protected GameObject counterImage;
    private Vector3[] rayDirections = new Vector3[3];   // 배열 선언과 동시에 크기 설정
    private int currentRayIndex = 0;                     // 현재 레이를 쏠 방향 인덱스
    public EnemyStateMachine stateMachine { get; private set; }
    public string lastAnimBoolName { get; private set; }
    public float moveSpeed = 1.5f;
    public float idleTime = 2;
    public float moveTime = 3;
    public float battleTime = 7;
    public float agroDistance = 5;

    // 방향벡터와 속도를 곱한 값으로 이동하는 함수
    public virtual void Move()
    {
        rb.velocity = moveDirection * moveSpeed;
    }

    // 몬스터 회전을 moveDirection 방향으로 설정하는 함수 (상태당 한 번씩)
    public virtual void RotateTowardsMoveDirection()
    {
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(targetRotation);
        }
    }

    // 정해진 랜덤 방향의 좌 우로 15도 한 값을 배열로 저장하는 함수 (상태당 한 번씩)
    public virtual void UpdateRayDirections()
    {
        if (moveDirection != Vector3.zero) // moveDirection이 유효할 때만 업데이트
        {
            rayDirections[0] = moveDirection;                                // 정면
            rayDirections[1] = Quaternion.Euler(0, -15, 0) * moveDirection;  // 왼쪽 15도
            rayDirections[2] = Quaternion.Euler(0, 15, 0) * moveDirection;   // 오른쪽 15도
        } 
    }

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new EnemyStateMachine(); 
    }
    public virtual EnemyState GetStateById(int stateId)
    {
        return null; // 기본적으로 null을 반환, 자식 클래스에서 구현 필요
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        if (Object.HasStateAuthority && stateMachine.currentState != null)
        {
            stateMachine.currentState.Update();
        }
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (Object.HasStateAuthority && stateMachine.currentState != null)
        {
            stateMachine.currentState.FixedUpdate();
        }
            
    }
    public virtual void AssignLastAnimName(string _animBoolName) => lastAnimBoolName = _animBoolName;
    public virtual void AnimationFinishTrigger() => stateMachine.currentState.AnimationFinishTrigger();
    public virtual void AnimationSpecialAttackTrigger()
    {
    }
    public Transform closestPlayerTransform = null; // 어그로 범위 내 가장 가까운 플레이어의 transform 저장
    public virtual Transform GetClosestPlayerWithinRange()
    {
        float closestDistance = float.MaxValue; // 가장 가까운 거리 비교용
        closestPlayerTransform = null; // 함수가 실행될 때마다 초기화

        foreach (var player in nearbyPlayerObjects)
        {
            if (player == null || player.gameObject == null || !player.gameObject.activeInHierarchy)
            {
                // player가 null이거나, gameObject가 파괴되었거나 비활성화된 경우 continue
                continue;
            }

            float distanceToPlayer = GetHorizontalDistance(transform.position, player.transform.position);

            // 어그로 범위 내에 있는 플레이어 중에서 가장 가까운 플레이어 찾기
            if (distanceToPlayer <= agroDistance && distanceToPlayer < closestDistance)
            {
                closestDistance = distanceToPlayer;
                closestPlayerTransform = player.transform; // 가장 가까운 어그로 범위 내 플레이어 업데이트
            }
        }

        // 가장 가까운 플레이어의 Transform 반환 (없으면 null)
        return closestPlayerTransform;
    }

    public virtual bool CheckAgroDistance()
    {
        if (closestPlayerTransform != null)
        {
            float distanceToPlayer = GetHorizontalDistance(transform.position, closestPlayerTransform.transform.position);
            if(distanceToPlayer <= agroDistance)
            {
                return true;
            }
        }        
        return false;
    }
    // 플레이어를 감지하는 어그로 범위 Ray로 쏘기 (플레이어 방향 구해서 agroDistance곱해서 쏘기)
    public virtual void DrawRayPlayerDirection()
    {
        foreach (var player in nearbyPlayerObjects)
        {
            Vector3 playerDir = (player.transform.position - transform.position).normalized;
            playerDir.y = 0; // Y축 값은 무시
            Debug.DrawRay(transform.position, playerDir * agroDistance, Color.green);
        }
    }

    // 각 방향에서 레이를 발사해서 장애물 검출하는 함수
    public virtual bool IsObstructed()
    {
        Ray ray = new Ray(Obstacle.position, rayDirections[currentRayIndex]); 
        RaycastHit hitData;
        Debug.DrawRay(ray.origin, ray.direction * ObstacleCheckDistance, Color.red);

        if (Physics.Raycast(ray, out hitData, ObstacleCheckDistance, ObstacleLayer))
        {
            Debug.DrawRay(ray.origin, ray.direction * hitData.distance, Color.green); 
            return true; // 장애물이 감지된 경우
        }
        
        currentRayIndex = (currentRayIndex + 1) % rayDirections.Length;
        return false; // 장애물이 없는 경우
    }
}
