using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
// Protected �� ����ϴ� ��쿡�� �ش�Ŭ������ �ڽ�Ŭ���������� ������ �����ؾ߸� �� �� ����. �ܺ������� �������� ������ ���� ����
public enum EnemyStateID
{
    Idle = 0,
    Move = 1,
    Battle = 2,
    // Add other states as needed
}
public class Entity : NetworkBehaviour
{

    public List<GameObject> nearbyPlayerObjects = new List<GameObject>();
    [Networked] public int NetworkedStateId { get; set; }

    [Networked, Capacity(12), OnChangedRender(nameof(OnNearbyPlayersChanged))]
    public NetworkLinkedList<PlayerRef> nearbyPlayers { get; } = new NetworkLinkedList<PlayerRef>();
  
 

    public Animator anim { get; private set; } //�ٸ� ��ũ��Ʈ���� entity.anim���� �ִϸ����Ϳ� �����Ͽ� �ִϸ��̼� ���¸� Ȯ���� �� ������, �ִϸ����͸� ������ ���� ����.
    public Rigidbody rb { get; private set; } // ��ƼƼ���� ���ͼ��� ������� �ܺμ����� �����ߴ� ���� Enemy���� rb = GetComponent<Rigidbody>(); �̷��ڵ� ���� �������� ������ rb.verocity �� ������ �����ϴ� �����Ǹ� �Ұ���

    public SpriteRenderer sr { get; private set; }
    public CapsuleCollider cd { get; private set; }

    [Header("Knockback info")]
    [SerializeField] protected Vector2 knockbackPower = new Vector2(7, 12);
    [SerializeField] protected Vector2 knockbackOffset = new Vector2(.5f, 2);
    [SerializeField] protected float knockbackDuration = .07f;
    
    protected bool isKnocked;

    [Header("Collision info")]
    public Transform attackCheck;
    public float attackCheckRadius = 1.2f;
    [SerializeField] protected Transform Obstacle;
    [SerializeField] protected float ObstacleCheckDistance = 5;
    [SerializeField] protected Transform wallCheck;
    [SerializeField] protected float wallCheckDistance = .8f;
    [SerializeField] protected LayerMask whatIsWall;
    PlayerSpawner playerSpawner;
    public int knockbackDir { get; private set; }
    public int facingDir { get; private set; } = 1;
    protected bool facingRight = true; 
    public System.Action onFlipped;

    #region MyNetwork
    
    [Networked, OnChangedRender(nameof(HealthChanged))] public float NetworkedHealth { get; set; } = 100;// ü�� ���� ��Ʈ��ũ �󿡼� ����ȭ�Ǹ� ������ �����Ǹ� HealthChanged ȣ��
    [Networked] protected Vector3 moveDirection { get; set; }
    public Transform target = null;

    // ������ ���� ���� �޼���
    // ���ʹϾ� ���� �װ�  xyzw, ���Ϸ��� xyz �ٵ� ���Ϸ��� ������ ������ ����  �׷��� Quaternion.Euler(x,x,x)�� ���Ϸ� ������ ���ʹϾ� ������ �ٲ� 

    public void SetRandomMoveDirection()
    {
        if (Object.HasStateAuthority)
        {
            float angleY = UnityEngine.Random.Range(0f, 360f);     // �� �� ���� ��
            Quaternion rotation = Quaternion.Euler(0, angleY, 0f); // �� ���Ϸ� ���� ���� ���ʹϾ����� ��ȯ�ؼ� ȸ���� ó���ϴ� ���Դϴ�. (������ �� ����)
            moveDirection = rotation * Vector3.forward;            // rotation �̰� �� 
        }
    }
    public virtual void OnEnterMoveState()
    {
        // Y �� ȸ���� �����Ͽ� �浹 �� ���� ������ ����
        rb.constraints = rb.constraints | RigidbodyConstraints.FreezeRotationY;

        // ��Ÿ �̵� ���·� ���� �� �ʿ��� ����
    }

    public virtual void OnExitMoveState()
    {
        // �ٸ� ���·� ��ȯ �� ȸ�� ������ ���� (�ʿ��)
        rb.constraints = RigidbodyConstraints.None;
    }
    private void OnNearbyPlayersChanged()
    {
        Debug.Log("???");
        nearbyPlayerObjects.Clear();

        // ��� PlayerRef�� ó��
        foreach (var playerRef in nearbyPlayers)
        {
            
                Debug.Log(playerRef + "Reset");
            
            StartCoroutine(AddPlayerObjectToList(playerRef));
        }
    }

    // ��Ʈ��ũ ��ü�� ��ȿ�� ������ ��� �� ����Ʈ�� �߰�
    private IEnumerator AddPlayerObjectToList(PlayerRef playerRef)
    {
        NetworkObject playerNetworkObject = null;

        // NetworkObject�� null�� �ƴ� ������ ���
        while (playerNetworkObject == null)
        {
            playerNetworkObject = Runner.GetPlayerObject(playerRef);
            if (playerNetworkObject == null)
            {
                Debug.LogWarning("Waiting for valid NetworkObject...");
            }
            yield return null; // ���� �����ӱ��� ���
        }

        // GameObject�� ����Ʈ�� �߰�
        if (playerNetworkObject.gameObject != null)
        {
            nearbyPlayerObjects.Add(playerNetworkObject.gameObject);
            Debug.Log($"Player {playerRef.PlayerId} added to the list.");
        }
    }
    protected virtual void Awake()
    {
        playerSpawner = FindObjectOfType<PlayerSpawner>();
        if (playerSpawner != null)
        {
            Debug.Log("PlayerSpawner found");

            // �̺�Ʈ ��� �� ����� �α� ���
            playerSpawner.OnPlayerJoined += AddPlayerToList;
            Debug.Log("OnPlayerJoined event registered");

            playerSpawner.OnPlayerLeft += RemovePlayerFromList;
            Debug.Log("OnPlayerLeft event registered");
        }
        else
        {
            Debug.LogWarning("PlayerSpawner not found");
        }

    }
    public virtual EnemyState GetStateById(int stateId)
    {
        return null; // �ڽ� Ŭ�������� ��üȭ �ʿ�
    }
    protected virtual void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
       // stats = GetComponent<CharacterStats>();
        cd = GetComponent<CapsuleCollider>();
        
    }
    // �÷��̾ nearbyPlayers ����Ʈ�� �߰�
    // Method to add a player to the nearbyPlayers list
    public void AddPlayerToList(PlayerRef player)
    {
        if (this == null || gameObject == null)
        {
            Debug.LogWarning("Entity has been destroyed. Cannot add player.");
            return;
        }

        StartCoroutine(WaitForPlayerInitialization(player));
    }

    // Coroutine to wait until the player's NetworkObject is valid before adding to the list
    private IEnumerator WaitForPlayerInitialization(PlayerRef player)
    {
        NetworkObject playerNetworkObject = null;

        // Wait until the player's NetworkObject is valid and initialized on the master client
        while (playerNetworkObject == null || !playerNetworkObject.IsValid)
        {
            playerNetworkObject = Runner.GetPlayerObject(player);
            if (playerNetworkObject == null)
            {
                Debug.LogWarning("Waiting for player NetworkObject to be valid...");
            }

            // Check if the parent object (this Entity or EnemyAi) has been destroyed
            if (this == null || gameObject == null)
            {
                Debug.LogWarning("Entity has been destroyed during coroutine. Exiting...");
                yield break; // Stop the coroutine if the object is destroyed
            }

            yield return null; // Wait until the next frame
        }

        // Now that the player's NetworkObject is valid, check if we have state authority and proceed
        if (Object.IsValid && Object.HasStateAuthority && !nearbyPlayers.Contains(player))
        {
            nearbyPlayers.Add(player); // Add the player to the networked list
            Debug.Log($"Player {player.PlayerId} added to nearbyPlayers.");
        }
    }

    // �÷��̾ nearbyPlayers ����Ʈ���� ����
    public void RemovePlayerFromList(PlayerRef player)
    {
        if (this == null || gameObject == null)
        {
            Debug.LogWarning("Entity has been destroyed. Cannot remove player.");
            return;
        }
        StartCoroutine(WaitForStateAuthorityAndRemovePlayer(player));

    }

    private IEnumerator WaitForStateAuthorityAndRemovePlayer(PlayerRef player)
    {
        if (this is EnemyAi enemyAi) // this�� EnemyAi���� Ȯ�� �� ĳ����
        {
            int stateId = NetworkedStateId;
            EnemyState newState = enemyAi.GetStateById(stateId); // EnemyAi�� GetStateById ȣ��
            while (!Object.HasStateAuthority || !Object.IsValid)
            {
                Debug.LogWarning("Waiting for state authority before changing state...");
                yield return null; // ���� �����ӱ��� ���
            }

            enemyAi.stateMachine.ChangeState(newState, stateId); // ���� ����
        }
        // Wait until this client has state authority
        while (!Object.HasStateAuthority || !Object.IsValid)
        {
            Debug.LogWarning("Waiting for state authority before modifying networked variables...");
            yield return null; // Wait for the next frame and try again
        }

        // Now we have state authority, so proceed
        if (Object.IsValid && nearbyPlayers.Contains(player))
        {
            Debug.Log($"Removing player {player.PlayerId} from nearbyPlayers.");
            nearbyPlayers.Remove(player);
        }
       
    }

    protected virtual void FixedUpdate()
    {

    }
    protected virtual void Update()
    {
        if (Object.IsValid)
        {
            Debug.Log("true");
        } else
        {
            Debug.Log("false");            
        }
        for (int i = nearbyPlayers.Count - 1; i >= 0; i--)
        {
            PlayerRef player = nearbyPlayers[i];
            if (!Runner.ActivePlayers.Contains(player))
            {
                Debug.Log($"Player {player.PlayerId} has left, removing from nearbyPlayers.");
                nearbyPlayers.Remove(player);
            }
        }

  
    }

    private void OnEnable()
    {
    }
 
  
    // ü���� ����Ǹ� ȣ���
    void HealthChanged()
    {
        Debug.Log($"Health changed to: {NetworkedHealth}");
        // ü���� ����� �� ü�¹ٳ� UI ������Ʈ ���� �ļ� �۾� ����
        UpdateHealthBar();
    }

    // ü�¹ٸ� ������Ʈ�ϴ� �Լ� (����)
    void UpdateHealthBar()
    {
        // ü�¹� UI ������Ʈ ����
        Debug.Log($"Updating health bar to: {NetworkedHealth}");
    }

    // RPC�� ���� State Authority Ŭ���̾�Ʈ���� ü���� ���ҽ�Ű�� �Լ�
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public virtual void DealDamageRpc(float damage)
    {
        // �� �ڵ�� State Authority Ŭ���̾�Ʈ������ �����
        if (Object.HasStateAuthority)
        {
            // ü���� ���ҽ�Ŵ
            NetworkedHealth -= damage;
            Debug.Log($"Monster damaged! Remaining Health: {NetworkedHealth}");

            // ü���� 0 ���ϰ� �Ǹ� ���͸� ����
            if (NetworkedHealth <= 0)
            {
                Die();
            }
        }
    }
    public void DestroyThis()
    {
      Runner.Despawn(Object); // Fusion�� Despawn ȣ��
    }
    public virtual void OnTriggerEnter(Collider col)
    {
        // �浹�� ��ü�� ����(��Ʈ) ��ü���� �±� Ȯ��
        Debug.Log("test");
        NetworkObject networkObject = col.GetComponentInParent<NetworkObject>();
        if (networkObject != null)
        {
            // NetworkObject�� ã�ҽ��ϴ�.
            // �� ��ü�� �÷��̾����� Ȯ���մϴ�.
            Character player = networkObject.GetComponent<Character>();
            if (player != null)
            {
                Debug.Log("�÷��̾ �����Ͽ����ϴ�.");
                // �ʿ��� ������ ó���մϴ�.                
            }
        }
    }
   
    public virtual void Die()
    {
        Debug.Log("Monster died.");
        // ��� ó�� ���� (��: ���� ����)
        Destroy(gameObject);
    }
    #endregion
    public virtual void SlowEntityBy(float _slowPercentage, float _slowDuration)
    {

    }

    protected virtual void ReturnDefaultSpeed()
    {
        anim.speed = 1;
    }

    /*public virtual void DamageImpact() => StartCoroutine("HitKnockback");

    public virtual void SetupKnockbackDir(Transform _damageDirection)
    {
        if (_damageDirection.position.x > transform.position.x)
            knockbackDir = -1;
        else if (_damageDirection.position.x < transform.position.x)
            knockbackDir = 1;
    }
    public void SetupKnockbackPower(Vector2 _knockbackpower) => knockbackPower = _knockbackpower;
    protected virtual IEnumerator HitKnockback()
    {
        isKnocked = true;

        float xOffset = Random.Range(knockbackOffset.x, knockbackOffset.y);


        if (knockbackPower.x > 0 || knockbackPower.y > 0) // This line makes player immune to freeze effect when he takes hit
            rb.velocity = new Vector2((knockbackPower.x + xOffset) * knockbackDir, knockbackPower.y);

        yield return new WaitForSeconds(knockbackDuration);
        isKnocked = false;
        SetupZeroKnockbackPower();
    }*/

    protected virtual void SetupZeroKnockbackPower()
    {

    }

    #region Velocity
    public void SetZeroVelocity()
    {
        rb.velocity = new Vector3(0, 0,0);
    }

    public void SetVelocity(float _xVelocity, float _yVelocity)
    {
        if (isKnocked)
            return;

        rb.velocity = new Vector2(_xVelocity, _yVelocity);
        FlipController(_xVelocity);
    }
    #endregion

    public virtual void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            // �÷��̾� ���� ��Ż
            target = null; // Ÿ�� ����
            Debug.Log("Player lost");
        }
    }
   /* public virtual bool IsGroundDetected() => Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
    public virtual bool IsWallDetected() => Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);*/

    public virtual void Flip()
    {
        facingDir = facingDir * -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);

        if (onFlipped != null)
            onFlipped();
    }

    public virtual void FlipController(float _x)
    {
        if (_x > 0 && !facingRight)
            Flip();
        else if (_x < 0 && facingRight)
            Flip();
    }
    public virtual void SetupDefailtFacingDir(int _direction)
    {
        facingDir = _direction;

        if (facingDir == -1)
            facingRight = false;
    }
   
}
