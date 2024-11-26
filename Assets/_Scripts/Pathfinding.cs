// Pathfinding.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinding : MonoBehaviour
{
    private Transform _target;  // 현재 타겟 (몬스터)
    public Transform target { 
        get => _target;
        set
        {
            if (_target == value)
            {
                 Debug.Log("타겟이 동일하여 변경되지 않음: " + _target?.name);
                return;
            }
            // 기존 타겟의 파티클 멈춤
            StopParticle(_target);

            // 새로운 타겟으로 설정
            _target = value;
            OpenMonsterInfoUI(target);

            // 새로운 타겟의 파티클 재생
            PlayParticle(_target);
        }


    }// 현재 타겟 (몬스터)
    
    private Grid grid;
    [SerializeField] private MouseManager mouseManager;

    // 경로가 업데이트될 때 호출되는 이벤트
    public event Action<List<Node>> OnPathUpdated;

    void Awake()
    {
        grid = FindObjectOfType<Grid>();
        //mouseManager = FindObjectOfType<MouseManager>();

        if (mouseManager != null)
        {
            mouseManager.OnMonsterClicked += SetTarget;
        }
        else
        {
             Debug.LogError("MouseManager를 찾을 수 없습니다.");
        }
    }

    void OnDestroy()
    {
        if (mouseManager != null)
        {
            mouseManager.OnMonsterClicked -= SetTarget;
        }
    }

    void SetTarget(Transform monsterTransform)
    {
        target = monsterTransform;
    }

    float findEvenCallTime = .1f;
    float timer = 0f;
    void Update()
    {
        if (target == null)
        {
            return;
        }

        timer += Time.deltaTime;
        if(findEvenCallTime < timer)
        {
            Debug.Log(timer);
            FindPath(transform.position, target.position);
            timer = 0f;
        }
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null)
        {
             Debug.LogError("시작 노드 또는 목표 노드가 null입니다.");
            OnPathUpdated?.Invoke(null);
            return;
        }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    openSet[i].fCost == currentNode.fCost &&
                    openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }

            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                    continue;

                int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

         Debug.LogWarning("경로를 찾지 못했습니다.");
        OnPathUpdated?.Invoke(null);
    }

    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
            if (currentNode == null)
            {
                 Debug.LogError("경로 추적 중 부모 노드가 null입니다.");
                OnPathUpdated?.Invoke(null);
                return;
            }
        }
        path.Reverse();

        grid.path = path;
        if(path.Count == 0) return;
         Debug.Log("경로가 생성되었습니다. 노드 수: " + path.Count);

        // 경로 업데이트 이벤트 호출
        OnPathUpdated?.Invoke(path);
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    private void StopParticle(Transform targetTransform)
    {
        if (targetTransform == null) return;

        var particle = targetTransform.GetComponent<Enemy>()?.ParticleManager?.Selector;
        if (particle != null && particle.isPlaying)
        {
            particle.gameObject.SetActive(false);
             Debug.Log("기존 타겟의 파티클 멈춤: " + targetTransform.name);
        }
    }

    private void PlayParticle(Transform targetTransform)
    {
        if (targetTransform == null) return;

        var particle = targetTransform.GetComponent<Enemy>()?.ParticleManager?.Selector;
        if (particle != null)
        {
            particle.gameObject.SetActive(true);
            particle.Play();
             Debug.Log("새로운 타겟 설정 및 파티클 재생: " + targetTransform.name);
        }
    }
    void OpenMonsterInfoUI(Transform target)
    {
        if(target == null) return;
        var chkObjActive = StaticManager.UI.EnemyInfoUI.gameObject;
        // 오브젝트가 비활성화 상태라면? 활성화
        if (!chkObjActive.activeSelf)
        {
            chkObjActive.SetActive(true);
        }


        EnemyInfoUI enemyUIComponent = chkObjActive.GetComponent<EnemyInfoUI>();
        if( target.gameObject.TryGetComponent(out Enemy componenet)){
            if(enemyUIComponent != null)
            {
                enemyUIComponent.Level.text = "Lv"+ componenet.Lv.ToString();
                float healthPercentage = (componenet.NetworkedHealth / componenet.MaxHealth) * 100f;
                enemyUIComponent.HpPercentText  = healthPercentage.ToString();
                enemyUIComponent.Slider.value = componenet.NetworkedHealth / componenet.MaxHealth;
                enemyUIComponent.ObjRef = componenet.transform;
            }
        }
        

    }

}
