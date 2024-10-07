using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;


public class Enemy : Entity
{
    [SerializeField] protected LayerMask ObstacleLayer;

    [Header("Stunned info")]
    /*public float stunDuration = 1;
    public Vector2 stunDirection = new Vector2(10, 12);
    protected bool canBeStunned;*/
    [SerializeField] protected GameObject counterImage;
    private Vector3[] rayDirections = new Vector3[3]; // �迭 ����� ���ÿ� ũ�� ����

    [Header("Move info")]
    public float moveSpeed = 1.5f;
    public float idleTime = 2;
    public float moveTime = 15;
    public float battleTime = 7;
    private float defaultMoveSpeed;
    private int currentRayIndex = 0; // ���� ���̸� �� ���� �ε���
    [Header("Attack info")]
    public float agroDistance = 2;
   
    [HideInInspector] public float lastTimeAttacked;

    // �̵� �޼���
    public virtual void Move()
    {
        rb.velocity = moveDirection * moveSpeed;
        RotateTowardsMoveDirection();
        UpdateRayDirections();

    }

    // ȸ�� �޼���
    public virtual void RotateTowardsMoveDirection()
    {
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = targetRotation;
        }
    }
    public virtual void UpdateRayDirections()
    {
        if (moveDirection != Vector3.zero) // moveDirection�� ��ȿ�� ���� ������Ʈ
        {
            rayDirections[0] = moveDirection; // ����
            rayDirections[1] = Quaternion.Euler(0, -15, 0) * moveDirection; // ���� 15��
            rayDirections[2] = Quaternion.Euler(0, 15, 0) * moveDirection; // ������ 15��
        }
        else
        {
            Debug.LogWarning("moveDirection is zero, rayDirections not updated.");
        }
    }

    public EnemyStateMachine stateMachine { get; private set; }
    //public EntityFX fx { get; private set; }
   // private Player player;
    public string lastAnimBoolName { get; private set; }
    protected override void Awake()
    {
        base.Awake();
        stateMachine = new EnemyStateMachine();

        defaultMoveSpeed = moveSpeed;
    }

    protected override void Start()
    {
        base.Start();

       // fx = GetComponent<EntityFX>();
    }

    protected override void Update()
    {
        base.Update();

        Debug.Log(stateMachine.currentState.ToString());
        stateMachine.currentState.Update();


    }

    public virtual void AssignLastAnimName(string _animBoolName) => lastAnimBoolName = _animBoolName;


    

    public virtual void FreezeTime(bool _timeFrozen)
    {
        if (_timeFrozen)
        {
            moveSpeed = 0;
            anim.speed = 0;
        }
        else
        {
            moveSpeed = defaultMoveSpeed;
            anim.speed = 1;
        }
    }

    public virtual void FreezeTimeFor(float _duration) => StartCoroutine(FreezeTimerCoroutine(_duration));

    protected virtual IEnumerator FreezeTimerCoroutine(float _seconds)
    {
        FreezeTime(true);

        yield return new WaitForSeconds(_seconds);

        FreezeTime(false);
    }

    #region Counter Attack Window
  /*  public virtual void OpenCounterAttackWindow()
    {
        canBeStunned = true;
        counterImage.SetActive(true);
    }*/

    /*public virtual void CloseCounterAttackWindow()
    {
        canBeStunned = false;
        counterImage.SetActive(false);
    }*/
    #endregion

   /* public virtual bool CanBeStunned()
    {
        if (canBeStunned)
        {
            CloseCounterAttackWindow();
            return true;
        }

        return false;
    }*/

    public virtual void AnimationFinishTrigger() => stateMachine.currentState.AnimationFinishTrigger();
    public virtual void AnimationSpecialAttackTrigger()
    {
    }
    public virtual bool IsPlayerWithinRange()
    {
        foreach (var player in nearbyPlayerObjects)
        {
            if (player != null) // �÷��̾� ������Ʈ�� ������ ���� �Ÿ� ���
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
                if (distanceToPlayer <= agroDistance)
                {
                    return true; // �÷��̾ Ư�� �Ÿ� ���� ���� �� true ��ȯ
                }
            }
        }
        return false; // Ư�� �Ÿ� ���� �÷��̾ ���� �� false ��ȯ
    }

    public virtual bool IsObstructed()
    {
        // ���� ���⿡ ���� ���� �߻�
        Ray ray = new Ray(Obstacle.position, rayDirections[currentRayIndex]); // �� ���⿡�� ���̸� �߻�
        RaycastHit hitData;

        // Debugging
        Debug.Log($"Ray Direction: {ray.direction}, Ray Origin: {ray.origin}");

        // Draw the ray in the scene view for debugging
        Debug.DrawRay(ray.origin, ray.direction * ObstacleCheckDistance, Color.red); // ���� �������� ���� �׸���

        // �浹 ����
        if (Physics.Raycast(ray, out hitData, ObstacleCheckDistance, ObstacleLayer))
        {
            Debug.Log("��ֹ� ����!");
            Debug.DrawRay(ray.origin, ray.direction * hitData.distance, Color.green); // �浹�� �������� �׸���
            return true; // ��ֹ��� ������ ���
        }

        // ���� ���� �������� ����
        currentRayIndex = (currentRayIndex + 1) % rayDirections.Length;

        return false; // ��ֹ��� ���� ���
    }
    /*  public virtual RaycastHit2D IsPlayerDetected()
      {
          RaycastHit2D playerDetected = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, 50, whatIsPlayer);
          RaycastHit2D wallDetected = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, 50, whatIsGround);

          if (wallDetected)
          {
              if (wallDetected.distance < playerDetected.distance)
                  return default(RaycastHit2D);
          }

          return playerDetected;
      }*/
    /*  protected override void OnDrawGizmos()
      {
          base.OnDrawGizmos();

          Gizmos.color = Color.yellow;
          Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + attackDistance * facingDir, transform.position.y));
      }*/
}
