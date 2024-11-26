using Fusion;
using Fusion.Addons.SimpleKCC;
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
}
public class Entity : NetworkBehaviour
{
    /*[SerializeField] private ParticleManager _particleManager;
    public ParticleManager ParticleManager { get => _particleManager; }
*/
    private MonsterManager _monsterManager;
    public void InitMonsterManager(MonsterManager refV)
    {
        _monsterManager = refV;
    }
    public List<GameObject> nearbyPlayerObjects = new List<GameObject>();
    [Networked] public int NetworkedStateId { get; set; }

    [Networked, Capacity(12), OnChangedRender(nameof(OnNearbyPlayersChanged))]
    public NetworkLinkedList<PlayerRef> nearbyPlayers { get; } = new NetworkLinkedList<PlayerRef>();

    public Animator anim { get; private set; } //다른 스크립트에서 entity.anim으로 애니메이터에 접근하여 애니메이션 상태를 확인할 수 있지만, 애니메이터를 변경할 수는 없다.
    public Rigidbody rb { get; private set; } // 엔티티에서 게터세터 사용으로 외부수정을 제한했다 만약 Enemy에서 rb = GetComponent<Rigidbody>(); 이런코드 쓰면 오류난다 하지만 rb.verocity 값 설정은 가능하다 재정의만 불가능

    public SpriteRenderer sr { get; private set; }
    public CapsuleCollider cd { get; private set; }


    [Header("Collision info")]
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
    [Networked] protected Vector3 moveDirection { get; set; }
    public Transform target = null;

    //  오브젝트가 앞으로 움직일 랜덤한 방향 
    public void SetRandomMoveDirection()
    {
        if (Object.HasStateAuthority)
        {
            float angleY = UnityEngine.Random.Range(0f, 360f);    
            Quaternion rotation = Quaternion.Euler(0, angleY, 0f); 
            moveDirection = rotation * Vector3.forward;           
        }
    }

    public float GetHorizontalDistance(Vector3 pos1, Vector3 pos2)
    {
        // Y값을 0으로 설정하여 XZ 평면의 거리만 계산
        pos1.y = 0;
        pos2.y = 0;
        return Vector3.Distance(pos1, pos2);
    }

    public virtual void OnEnterMoveState()
    {
        // Y 축 회전을 고정하여 충돌 시 떨림 현상을 방지
        rb.constraints = rb.constraints | RigidbodyConstraints.FreezeRotationY;
    }

    public virtual void OnExitMoveState()
    {
        rb.constraints = RigidbodyConstraints.None; // 다른 상태로 전환 시 회전 고정을 해제 (필요시)
    }
    private void OnNearbyPlayersChanged()
    {        
        nearbyPlayerObjects.Clear();
        foreach (var playerRef in nearbyPlayers) // 모든 PlayerRef를 처리
        {
            StartCoroutine(AddPlayerObjectToList(playerRef));
        }
    }
    private IEnumerator AddPlayerObjectToList(PlayerRef playerRef) // 네트워크 객체가 유효할 때까지 대기 후 리스트에 추가
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
                //TEMPHIDE// Debug.LogWarning("Waiting for player NetworkObject to be valid...");
            }

            // Check if the parent object (this Entity or EnemyAi) has been destroyed
            if (this == null || gameObject == null)
            {
                //TEMPHIDE// Debug.LogWarning("Entity has been destroyed during coroutine. Exiting...");
                yield break; // Stop the coroutine if the object is destroyed
            }

            yield return null; // Wait until the next frame
        }

        // Now that the player's NetworkObject is valid, check if we have state authority and proceed
        if (Object.IsValid && Object.HasStateAuthority && !nearbyPlayers.Contains(player))
        {
            nearbyPlayers.Add(player); // Add the player to the networked list
             //TEMPHIDE// Debug.Log($"Player {player.PlayerId} added to nearbyPlayers.");
        }
    }

    // 플레이어를 nearbyPlayers 리스트에서 제거
    public void RemovePlayerFromList(PlayerRef player)
    {
        if (this == null || gameObject == null)
        {
            //TEMPHIDE// Debug.LogWarning("Entity has been destroyed. Cannot remove player.");
            return;
        }
        StartCoroutine(WaitForStateAuthorityAndRemovePlayer(player));

    }

    private IEnumerator WaitForStateAuthorityAndRemovePlayer(PlayerRef player)
    {
        if (this is EnemyAi enemyAi) // this가 EnemyAi인지 확인 후 캐스팅
        {
            while (!Object.HasStateAuthority || !Object.IsValid)
            {
                //TEMPHIDE// Debug.LogWarning("Waiting for state authority before changing state...");
                yield return null; // 다음 프레임까지 대기
            }

            enemyAi.stateMachine.ChangeState(enemyAi.idleState); // 상태 변경
            enemyAi.gameObject.SetActive(false);
            enemyAi.gameObject.SetActive(true);
        }
        // Wait until this client has state authority
        while (!Object.HasStateAuthority || !Object.IsValid)
        {
            //TEMPHIDE//  Debug.LogWarning("Waiting for state authority before modifying networked variables...");
            yield return null; // Wait for the next frame and try again
        }

        // Now we have state authority, so proceed
        if (Object.IsValid && nearbyPlayers.Contains(player))
        {
            //Debug.Log($"Removing player {player.PlayerId} from nearbyPlayers.");
            nearbyPlayers.Remove(player);
        }
       
    }

    protected virtual void FixedUpdate()
    {

    }

    protected virtual void Update()
    {
        for (int i = nearbyPlayers.Count - 1; i >= 0; i--)
        {
            PlayerRef player = nearbyPlayers[i];
            if (!Runner.ActivePlayers.Contains(player))
            {
                //Debug.Log($"Player {player.PlayerId} has left, removing from nearbyPlayers.");
                nearbyPlayers.Remove(player);
            }
        }
    }

    private void OnEnable()
    {
    }
 
  
    // 체력이 변경되면 호출됨


    // RPC를 통해 State Authority 클라이언트에서 체력을 감소시키는 함수
    
    public void DestroyThis()
    {
      Runner.Despawn(Object); // Fusion의 Despawn 호출
    }
    public virtual void OnTriggerEnter(Collider col)
    {
        // 충돌한 객체의 상위(루트) 객체에서 태그 확인
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
        if (_monsterManager == null) _monsterManager = FindObjectOfType<MonsterManager>();
        _monsterManager.DespawnMonster(gameObject.GetComponent<NetworkObject>());
    }
    #endregion

    protected virtual void ReturnDefaultSpeed()
    {
        anim.speed = 1;
    }
    protected virtual void SetupZeroKnockbackPower()
    {

    }

    #region Velocity
    public void SetZeroVelocity()
    {
        rb.velocity = new Vector3(0, 0,0);
    }

    #endregion

    public virtual void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            // 플레이어 범위 이탈
            target = null; // 타겟 해제
            
        }
    }
}
