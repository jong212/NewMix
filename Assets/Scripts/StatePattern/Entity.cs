using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
// Protected 를 사용하는 경우에는 해당클래스랑 자식클래스에서만 수정이 가능해야만 할 때 쓰자. 외부참조로 무변별한 수정을 막기 위함
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
  
 

    public Animator anim { get; private set; } //다른 스크립트에서 entity.anim으로 애니메이터에 접근하여 애니메이션 상태를 확인할 수 있지만, 애니메이터를 변경할 수는 없다.
    public Rigidbody rb { get; private set; } // 엔티티에서 게터세터 사용으로 외부수정을 제한했다 만약 Enemy에서 rb = GetComponent<Rigidbody>(); 이런코드 쓰면 오류난다 하지만 rb.verocity 값 설정은 가능하다 재정의만 불가능

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
    
    [Networked, OnChangedRender(nameof(HealthChanged))] public float NetworkedHealth { get; set; } = 100;// 체력 값이 네트워크 상에서 동기화되며 변경이 감지되면 HealthChanged 호출
    [Networked] protected Vector3 moveDirection { get; set; }
    public Transform target = null;

    // 랜덤한 방향 설정 메서드
    // 쿼터니언 각도 네개  xyzw, 오일러는 xyz 근데 오일러는 짐벌락 현상이 있음  그래서 Quaternion.Euler(x,x,x)로 오일러 각도를 쿼터니언 각도로 바꿈 

    public void SetRandomMoveDirection()
    {
        if (Object.HasStateAuthority)
        {
            float angleY = UnityEngine.Random.Range(0f, 360f);     // 좌 우 랜덤 값
            Quaternion rotation = Quaternion.Euler(0, angleY, 0f); // 이 오일러 각도 값을 쿼터니언으로 변환해서 회전을 처리하는 것입니다. (짐벌락 안 생김)
            moveDirection = rotation * Vector3.forward;            // rotation 이건 딱 
        }
    }
    public virtual void OnEnterMoveState()
    {
        // Y 축 회전을 고정하여 충돌 시 떨림 현상을 방지
        rb.constraints = rb.constraints | RigidbodyConstraints.FreezeRotationY;

        // 기타 이동 상태로 진입 시 필요한 로직
    }

    public virtual void OnExitMoveState()
    {
        // 다른 상태로 전환 시 회전 고정을 해제 (필요시)
        rb.constraints = RigidbodyConstraints.None;
    }
    private void OnNearbyPlayersChanged()
    {
        Debug.Log("???");
        nearbyPlayerObjects.Clear();

        // 모든 PlayerRef를 처리
        foreach (var playerRef in nearbyPlayers)
        {
            
                Debug.Log(playerRef + "Reset");
            
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
            if (playerNetworkObject == null)
            {
                Debug.LogWarning("Waiting for valid NetworkObject...");
            }
            yield return null; // 다음 프레임까지 대기
        }

        // GameObject를 리스트에 추가
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
    public virtual EnemyState GetStateById(int stateId)
    {
        return null; // 자식 클래스에서 구체화 필요
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

    // 플레이어를 nearbyPlayers 리스트에서 제거
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
        if (this is EnemyAi enemyAi) // this가 EnemyAi인지 확인 후 캐스팅
        {
            int stateId = NetworkedStateId;
            EnemyState newState = enemyAi.GetStateById(stateId); // EnemyAi의 GetStateById 호출
            while (!Object.HasStateAuthority || !Object.IsValid)
            {
                Debug.LogWarning("Waiting for state authority before changing state...");
                yield return null; // 다음 프레임까지 대기
            }

            enemyAi.stateMachine.ChangeState(newState, stateId); // 상태 변경
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
    public void DestroyThis()
    {
      Runner.Despawn(Object); // Fusion의 Despawn 호출
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
            // 플레이어 범위 이탈
            target = null; // 타겟 해제
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
