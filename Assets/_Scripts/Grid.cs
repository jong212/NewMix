using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public LayerMask unwalkableMask;    // 이동 불가능한 레이어 마스크
    public Vector2 gridWorldSize;       // 그리드의 월드 크기 (가로, 세로)
    public float nodeRadius = 0.5f;     // 노드의 반지름 (기본값 0.5)

    public List<Node> path;             // 현재 경로를 저장할 리스트

    Node[,] grid;                        // 노드 배열

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    void Awake()
    {
        // nodeRadius가 0 이하로 설정되지 않도록 확인
        if (nodeRadius <= 0)
        {
            nodeRadius = 0.5f;
        }

        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        // gridSizeX와 gridSizeY가 양수인지 확인
        if (gridSizeX <= 0 || gridSizeY <= 0)
        {
            //Debug.LogError("gridSizeX 또는 gridSizeY가 0 이하입니다. gridWorldSize와 nodeRadius를 확인하세요.");
            return;
        }

        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position -
            Vector3.right * gridWorldSize.x / 2 -
            Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft +
                    Vector3.right * (x * nodeDiameter + nodeRadius) +
                    Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    // 월드 좌표를 그리드 상의 노드로 변환
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        if (grid == null)
        {
           // Debug.LogError("Grid가 초기화되지 않았습니다.");
            return null;
        }

        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        x = Mathf.Clamp(x, 0, gridSizeX - 1);
        y = Mathf.Clamp(y, 0, gridSizeY - 1);

        return grid[x, y];
    }

    // 이웃 노드 가져오기 (8방향 탐색)
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // 자기 자신은 제외
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                // 그리드 범위 내인지 확인
                if (checkX >= 0 && checkX < gridSizeX &&
                    checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    // TEMP 테스트 끝나면 주석처리 하기
    /*void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                if (path != null && path.Contains(n))
                {
                    Gizmos.color = Color.black; // 경로 상의 노드는 검은색
                }
                *//*Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f)); 씬 그리드 원래코드인데 아래 코드 이상한면 이걸로 사용*//*
                Gizmos.DrawCube(new Vector3(n.worldPosition.x, transform.position.y, n.worldPosition.z), Vector3.one * (nodeDiameter - 0.05f));

            }

            // 경로를 선으로 그리기
            if (path != null && path.Count > 1)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Gizmos.DrawLine(path[i].worldPosition, path[i + 1].worldPosition);
                }
            }
        }
    }*/
}
