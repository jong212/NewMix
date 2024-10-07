using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Entity : NetworkBehaviour
{
    public List<GameObject> nearbyPlayerObjects = new List<GameObject>();

    [Networked, Capacity(12), OnChangedRender(nameof(OnNearbyPlayersChanged))]
    public NetworkLinkedList<PlayerRef> nearbyPlayers { get; } = new NetworkLinkedList<PlayerRef>();
  
    private void OnNearbyPlayersChanged()
    {
        // ����Ʈ�� Ŭ�����ϰ� �ٽ� ����
        nearbyPlayerObjects.Clear();

        // ��� PlayerRef�� ó��
        foreach (var playerRef in nearbyPlayers)
        {
            if (Object.HasStateAuthority)
            {
                Debug.Log(playerRef + "Reset");
            }
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
            yield return null; // ���� �����ӱ��� ���
        }

        // GameObject�� ����Ʈ�� �߰�
        nearbyPlayerObjects.Add(playerNetworkObject.gameObject);
        Debug.Log($"Player {playerRef.PlayerId} added to the list.");
    }

    public Animator anim { get; private set; }
    public Rigidbody rb { get; private set; }
 
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
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected float groundCheckDistance = 1;
    [SerializeField] protected Transform wallCheck;
    [SerializeField] protected float wallCheckDistance = .8f;
    [SerializeField] protected LayerMask whatIsGround;
    PlayerSpawner playerSpawner;
    public int knockbackDir { get; private set; }
    public int facingDir { get; private set; } = 1;
    protected bool facingRight = true; 
    public System.Action onFlipped;

    #region MyNetwork
    // ü�� ���� ��Ʈ��ũ �󿡼� ����ȭ�Ǹ� ������ �����Ǹ� HealthChanged ȣ��
    [Networked, OnChangedRender(nameof(HealthChanged))]
    public float NetworkedHealth { get; set; } = 100;
    public Transform target = null;

    
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

    protected virtual void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
       // stats = GetComponent<CharacterStats>();
        cd = GetComponent<CapsuleCollider>();
        
    }
    // �÷��̾ nearbyPlayers ����Ʈ�� �߰�
    public void AddPlayerToList(PlayerRef player)
    {
        if (!Object.IsValid)
        {
            Debug.LogWarning("Network object is not spawned yet. Cannot add player.");
            return;
        }

        // nearbyPlayers�� �̹� �ش� �÷��̾ ������ �߰�
        if (!nearbyPlayers.Contains(player))
        {
            nearbyPlayers.Add(player);
            Debug.Log($"Player {player.PlayerId} added to nearbyPlayers.");
        }
    }

    // �÷��̾ nearbyPlayers ����Ʈ���� ����
    public void RemovePlayerFromList(PlayerRef player)
    {
        if (!Object.IsValid)
        {
            Debug.LogWarning("Entity is not valid or not spawned yet. Cannot remove player.");
            return; // ��ü�� ��ȿ���� �ʰų� ���� �������� �ʾ����� ����
        }

        // nearbyPlayers ����Ʈ���� �÷��̾ ����
        if (nearbyPlayers.Contains(player))
        {
            Debug.Log("LeftTest : " + "3");

            // NetworkObject�� ��ȿ���� ������ Ȯ�� �� ����
            NetworkObject playerNetworkObject = Runner.GetPlayerObject(player);
            if (playerNetworkObject == null || playerNetworkObject.gameObject == null)
            {
                Debug.LogWarning($"Player {player.PlayerId} is missing or destroyed. Removing from list.");
            }

            nearbyPlayers.Remove(player);
            Debug.Log($"Player {player.PlayerId} removed from nearbyPlayers.");
        }
    }

    protected virtual void Update()
    {

        for (int i = nearbyPlayers.Count - 1; i >= 0; i--)
        {
            PlayerRef player = nearbyPlayers[i];
            if (!Runner.ActivePlayers.Contains(player))
            {
                Debug.Log($"Player {player.PlayerId} has left, removing from nearbyPlayers.");
                nearbyPlayers.Remove(player);
            }
        }

        // nearbyPlayerObjects ����Ʈ�� �ִ� ��� ������Ʈ�� ������ ���
        foreach (var playerObject in nearbyPlayerObjects)
        {
            Debug.Log($"Player Object: {playerObject.GetInstanceID()}");
        }

        // nearbyPlayers ����Ʈ�� �ִ� PlayerRef ������ ���
        foreach (var playerRef in nearbyPlayers)
        {
            Debug.Log($"PlayerRef in nearbyPlayers: {playerRef.PlayerId}");
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

    public virtual void DamageImpact() => StartCoroutine("HitKnockback");

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
    }

    protected virtual void SetupZeroKnockbackPower()
    {

    }

    #region Velocity
    public void SetZeroVelocity()
    {
        if (isKnocked)
            return;

        rb.velocity = new Vector2(0, 0);
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
    public virtual bool IsGroundDetected() => Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
    public virtual bool IsWallDetected() => Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);

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
