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
        // 리스트를 클리어하고 다시 갱신
        nearbyPlayerObjects.Clear();

        // 모든 PlayerRef를 처리
        foreach (var playerRef in nearbyPlayers)
        {
            if (Object.HasStateAuthority)
            {
                Debug.Log(playerRef + "Reset");
            }
            StartCoroutine(AddPlayerObjectToList(playerRef));
        }
    }

    // 네트워크 객체가 유효할 때까지 대기 후 리스트에 추가
    private IEnumerator AddPlayerObjectToList(PlayerRef playerRef)
    {
        NetworkObject playerNetworkObject = null;

        // NetworkObject가 null이 아닐 때까지 대기
        while (playerNetworkObject == null)
        {
            playerNetworkObject = Runner.GetPlayerObject(playerRef);
            yield return null; // 다음 프레임까지 대기
        }

        // GameObject를 리스트에 추가
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
    // 체력 값이 네트워크 상에서 동기화되며 변경이 감지되면 HealthChanged 호출
    [Networked, OnChangedRender(nameof(HealthChanged))]
    public float NetworkedHealth { get; set; } = 100;
    public Transform target = null;

    
    protected virtual void Awake()
    {
        playerSpawner = FindObjectOfType<PlayerSpawner>();
        if (playerSpawner != null)
        {
            Debug.Log("PlayerSpawner found");

            // 이벤트 등록 시 디버그 로그 출력
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
    // 플레이어를 nearbyPlayers 리스트에 추가
    public void AddPlayerToList(PlayerRef player)
    {
        if (!Object.IsValid)
        {
            Debug.LogWarning("Network object is not spawned yet. Cannot add player.");
            return;
        }

        // nearbyPlayers에 이미 해당 플레이어가 없으면 추가
        if (!nearbyPlayers.Contains(player))
        {
            nearbyPlayers.Add(player);
            Debug.Log($"Player {player.PlayerId} added to nearbyPlayers.");
        }
    }

    // 플레이어를 nearbyPlayers 리스트에서 제거
    public void RemovePlayerFromList(PlayerRef player)
    {
        if (!Object.IsValid)
        {
            Debug.LogWarning("Entity is not valid or not spawned yet. Cannot remove player.");
            return; // 객체가 유효하지 않거나 아직 스폰되지 않았으면 종료
        }

        // nearbyPlayers 리스트에서 플레이어를 제거
        if (nearbyPlayers.Contains(player))
        {
            Debug.Log("LeftTest : " + "3");

            // NetworkObject가 유효하지 않은지 확인 후 제거
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

        // nearbyPlayerObjects 리스트에 있는 모든 오브젝트의 정보를 출력
        foreach (var playerObject in nearbyPlayerObjects)
        {
            Debug.Log($"Player Object: {playerObject.GetInstanceID()}");
        }

        // nearbyPlayers 리스트에 있는 PlayerRef 정보를 출력
        foreach (var playerRef in nearbyPlayers)
        {
            Debug.Log($"PlayerRef in nearbyPlayers: {playerRef.PlayerId}");
        }
    }

    private void OnEnable()
    {
    }
 
  
    // 체력이 변경되면 호출됨
    void HealthChanged()
    {
        Debug.Log($"Health changed to: {NetworkedHealth}");
        // 체력이 변경될 때 체력바나 UI 업데이트 등의 후속 작업 수행
        UpdateHealthBar();
    }

    // 체력바를 업데이트하는 함수 (예시)
    void UpdateHealthBar()
    {
        // 체력바 UI 업데이트 로직
        Debug.Log($"Updating health bar to: {NetworkedHealth}");
    }

    // RPC를 통해 State Authority 클라이언트에서 체력을 감소시키는 함수
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public virtual void DealDamageRpc(float damage)
    {
        // 이 코드는 State Authority 클라이언트에서만 실행됨
        if (Object.HasStateAuthority)
        {
            // 체력을 감소시킴
            NetworkedHealth -= damage;
            Debug.Log($"Monster damaged! Remaining Health: {NetworkedHealth}");

            // 체력이 0 이하가 되면 몬스터를 죽임
            if (NetworkedHealth <= 0)
            {
                Die();
            }
        }
    }
    public virtual void OnTriggerEnter(Collider col)
    {
        // 충돌한 객체의 상위(루트) 객체에서 태그 확인
        Debug.Log("test");
        NetworkObject networkObject = col.GetComponentInParent<NetworkObject>();
        if (networkObject != null)
        {
            // NetworkObject를 찾았습니다.
            // 이 객체가 플레이어인지 확인합니다.
            Character player = networkObject.GetComponent<Character>();
            if (player != null)
            {
                Debug.Log("플레이어를 감지하였습니다.");
                // 필요한 로직을 처리합니다.                
            }
        }
    }
   
    public virtual void Die()
    {
        Debug.Log("Monster died.");
        // 사망 처리 로직 (예: 몬스터 제거)
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
            // 플레이어 범위 이탈
            target = null; // 타겟 해제
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
