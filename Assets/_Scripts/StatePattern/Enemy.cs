using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���⼭���� ������Ʈ�� ���� �� > ���� ������Ʈ ġ�� > ���� ���� ������Ʈ ���� ġ�� > �׷��� update, fixedupdae �߰����� 
public class Enemy : Entity
{
    [SerializeField] protected LayerMask ObstacleLayer;
    private Vector3[] rayDirections = new Vector3[3];            // �迭 ����� ���ÿ� ũ�� ����
    private int currentRayIndex = 0;                             // ���� ���̸� �� ���� �ε���
    public EnemyStateMachine stateMachine { get; private set; }

    protected float agroDistance = 5;                            // �÷��̾� ���� �Ÿ�
    public float attackDistance = 3;                             // ���� ���Ÿ� ���� �ٸ��� ������ ��
    public float attackCooldown = 1;                             // ���� ���� ��Ÿ��
    public float moveSpeed = 1.5f;                               // ���� �̼�
    public float idleTime = 2;                                   // ���� ���� �ð�
    public float moveTime = 3;                                   // ���� ���� �ð�
    public float battleTime = 7;                                 // ���� ���� �ð�
    [HideInInspector] public float lastTimeAttacked;

    public string lastAnimBoolName { get; private set; }

    // ���⺤�Ϳ� �ӵ��� ���� ������ �̵��ϴ� �Լ�
    public virtual void Move()
    {
        rb.velocity = moveDirection * moveSpeed;
    }

    // ���� ȸ���� moveDirection �������� �����ϴ� �Լ� (���´� �� ����)
    public virtual void RotateTowardsMoveDirection()
    {
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(targetRotation);
        }
    }

    // ������ ���� ������ �� ��� 15�� �� ���� �迭�� �����ϴ� �Լ� (���´� �� ����)
    public virtual void UpdateRayDirections()
    {
        if (moveDirection != Vector3.zero) // moveDirection�� ��ȿ�� ���� ������Ʈ
        {
            rayDirections[0] = moveDirection;                                // ����
            rayDirections[1] = Quaternion.Euler(0, -15, 0) * moveDirection;  // ���� 15��
            rayDirections[2] = Quaternion.Euler(0, 15, 0) * moveDirection;   // ������ 15��
        } 
    }

    public virtual void TargetMoveMonster()
    {
        Vector3 directionToPlayer = (closestPlayerTransform.position - transform.position).normalized; // ���� ����
        directionToPlayer.y = 0;                                                                       // Y�� ���� ��� �̵�
        rb.velocity = directionToPlayer * moveSpeed;                                                   // �������� �ӵ���ŭ �̵�

        if (directionToPlayer != Vector3.zero)                                                         // ���� ���Ͱ� ��ȿ���� Ȯ��
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);                    // �̵� ������ ���� ȸ���� ����
            rb.MoveRotation(targetRotation);                                                           // ȸ������ �����Ͽ� ȸ��
        }
    }

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new EnemyStateMachine(); 
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
    public Transform closestPlayerTransform = null; // ��׷� ���� �� ���� ����� �÷��̾��� transform ����
    public virtual Transform GetClosestPlayerWithinRange()
    {
        float closestDistance = float.MaxValue; // ���� ����� �Ÿ� �񱳿�
        closestPlayerTransform = null; // �Լ��� ����� ������ �ʱ�ȭ

        foreach (var player in nearbyPlayerObjects)
        {
            if (player == null || player.gameObject == null || !player.gameObject.activeInHierarchy)
            {
                // player�� null�̰ų�, gameObject�� �ı��Ǿ��ų� ��Ȱ��ȭ�� ��� continue
                continue;
            }

            float distanceToPlayer = GetHorizontalDistance(transform.position, player.transform.position);

            // ��׷� ���� ���� �ִ� �÷��̾� �߿��� ���� ����� �÷��̾� ã��
            if (distanceToPlayer <= agroDistance && distanceToPlayer < closestDistance)
            {
                closestDistance = distanceToPlayer;
                closestPlayerTransform = player.transform; // ���� ����� ��׷� ���� �� �÷��̾� ������Ʈ
            }
        }

        // ���� ����� �÷��̾��� Transform ��ȯ (������ null)
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
    // �÷��̾ �����ϴ� ��׷� ���� Ray�� ��� (�÷��̾� ���� ���ؼ� agroDistance���ؼ� ���)
    public virtual void DrawRayPlayerDirection()
    {
        foreach (var player in nearbyPlayerObjects)
        {
            if(player == null) continue;

            Vector3 playerDir = (player.transform.position - transform.position).normalized;
            playerDir.y = 0; // Y�� ���� ����
            Debug.DrawRay(transform.position, playerDir * agroDistance, Color.green);
        }
    }

    // �� ���⿡�� ���̸� �߻��ؼ� ��ֹ� �����ϴ� �Լ�
    public virtual bool IsObstructed()
    {
        Ray ray = new Ray(Obstacle.position, rayDirections[currentRayIndex]); 
        RaycastHit hitData;
        Debug.DrawRay(ray.origin, ray.direction * ObstacleCheckDistance, Color.red);

        if (Physics.Raycast(ray, out hitData, ObstacleCheckDistance, ObstacleLayer))
        {
            Debug.DrawRay(ray.origin, ray.direction * hitData.distance, Color.green); 
            return true; // ��ֹ��� ������ ���
        }
        
        currentRayIndex = (currentRayIndex + 1) % rayDirections.Length;
        return false; // ��ֹ��� ���� ���
    }
}
