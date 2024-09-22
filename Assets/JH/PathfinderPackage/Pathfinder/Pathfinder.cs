using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinder : MonoBehaviour
{
    // 경로 탐색에 사용할 타일맵 (유니티 에디터에서 할당)
    public Tilemap tilemap;

    // A* 알고리즘을 사용하여 경로를 찾는 함수
    public void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        // 시작 위치와 목표 위치를 타일맵의 셀 좌표로 변환
        Vector3Int startCell = tilemap.WorldToCell(startPos);
        Vector3Int targetCell = tilemap.WorldToCell(targetPos);

        // Z축 좌표를 0으로 설정하여 2D 평면에서 계산되도록 함
        startCell.z = 0;
        targetCell.z = 0;

        // 시작 셀과 목표 셀 정보를 로그로 출력
        Debug.Log($"Start Cell: {startCell}, Target Cell: {targetCell}");

        // 목표 셀에 타일이 없거나, 이동 불가능한 타일이면 경로 탐색 중지
        if (!tilemap.HasTile(targetCell) || !IsTilePassable(tilemap.GetTile(targetCell)))
        {
            Debug.Log("No tile found at target cell or tile is impassable, stopping pathfinding.");
            return;
        }

        // A* 알고리즘에 필요한 데이터 구조 초기화
        List<Vector3Int> openList = new List<Vector3Int>(); // 탐색할 셀 목록
        HashSet<Vector3Int> closedList = new HashSet<Vector3Int>(); // 이미 탐색한 셀 목록
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>(); // 각 셀에 도달하기 직전의 셀
        Dictionary<Vector3Int, float> gScore = new Dictionary<Vector3Int, float>(); // 시작 지점부터 해당 셀까지의 실제 비용
        Dictionary<Vector3Int, float> fScore = new Dictionary<Vector3Int, float>(); // gScore + 휴리스틱 추정 비용

        // 시작 셀을 openList에 추가하고, gScore와 fScore 초기화
        openList.Add(startCell);
        gScore[startCell] = 0; // 시작 지점의 gScore는 0
        fScore[startCell] = HeuristicCostEstimate(startCell, targetCell); // fScore는 휴리스틱 추정값으로 초기화

        // openList에 탐색할 셀이 남아있는 동안 반복
        while (openList.Count > 0)
        {
            // fScore가 가장 낮은 셀을 선택하여 현재 셀로 설정
            Vector3Int current = GetLowestFScore(openList, fScore);

            // 현재 셀이 목표 셀과 동일한지 확인
            if (current.x == targetCell.x && current.y == targetCell.y)
            {
                Debug.Log("Retracing path to move to target.");
                // 경로를 복원하고 이동 시작
                RetracePath(cameFrom, current);
                return;
            }

            // 현재 셀을 openList에서 제거하고 closedList에 추가
            openList.Remove(current);
            closedList.Add(current);

            // 현재 셀의 이웃 셀들을 검사
            foreach (Vector3Int neighbor in GetNeighbors(current))
            {
                // 이미 탐색한 셀은 무시
                if (closedList.Contains(neighbor))
                    continue;

                // 이웃 셀의 타일 가져오기
                TileBase tile = tilemap.GetTile(neighbor);

                // 타일이 없거나, 이동 불가능한 타일이면 무시
                if (tile == null || !IsTilePassable(tile))
                {
                    Debug.Log($"Skipping neighbor: {neighbor}, tile is impassable.");
                    continue;
                }

                // 현재 셀을 통해 이웃 셀로 가는 비용 계산 (여기서는 가중치를 모두 1로 설정)
                float tentativeGScore = gScore[current] + 1;

                // 이웃 셀이 openList에 없거나, 더 짧은 경로를 발견한 경우
                if (!openList.Contains(neighbor) || tentativeGScore < gScore.GetValueOrDefault(neighbor, Mathf.Infinity))
                {
                    // 이웃 셀로 가는 최단 경로 갱신
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, targetCell);

                    // openList에 없으면 추가
                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        // 경로를 찾지 못한 경우
        Debug.Log("No valid path found.");
    }

    // 휴리스틱 함수: 현재 셀에서 목표 셀까지의 추정 비용 계산 (맨해튼 거리 사용)
    private float HeuristicCostEstimate(Vector3Int start, Vector3Int goal)
    {
        return Mathf.Abs(goal.x - start.x) + Mathf.Abs(goal.y - start.y);
    }

    // fScore가 가장 낮은 셀을 openList에서 선택하는 함수
    private Vector3Int GetLowestFScore(List<Vector3Int> openList, Dictionary<Vector3Int, float> fScore)
    {
        Vector3Int lowest = openList[0];
        float lowestScore = fScore[lowest];

        // openList의 각 셀에 대해 fScore를 비교하여 최소값 찾기
        foreach (Vector3Int cell in openList)
        {
            if (fScore.ContainsKey(cell) && fScore[cell] < lowestScore)
            {
                lowest = cell;
                lowestScore = fScore[cell];
            }
        }

        return lowest;
    }

    // 현재 셀의 인접한 이웃 셀들의 리스트를 반환하는 함수
    private List<Vector3Int> GetNeighbors(Vector3Int cell)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>();

        // 상하좌우 이웃 추가
        neighbors.Add(new Vector3Int(cell.x + 1, cell.y, cell.z)); // 오른쪽
        neighbors.Add(new Vector3Int(cell.x - 1, cell.y, cell.z)); // 왼쪽
        neighbors.Add(new Vector3Int(cell.x, cell.y + 1, cell.z)); // 위쪽
        neighbors.Add(new Vector3Int(cell.x, cell.y - 1, cell.z)); // 아래쪽

        // 대각선 이웃 추가 (필요한 경우)
        neighbors.Add(new Vector3Int(cell.x + 1, cell.y + 1, cell.z)); // 오른쪽 위
        neighbors.Add(new Vector3Int(cell.x - 1, cell.y + 1, cell.z)); // 왼쪽 위
        neighbors.Add(new Vector3Int(cell.x + 1, cell.y - 1, cell.z)); // 오른쪽 아래
        neighbors.Add(new Vector3Int(cell.x - 1, cell.y - 1, cell.z)); // 왼쪽 아래

        return neighbors;
    }

    // 타일의 이동 가능 여부를 확인하는 함수
    private bool IsTilePassable(TileBase tile)
    {
        // 타일을 CustomTile로 캐스팅하여 isPassable 속성 확인
        CustomTile customTile = tile as CustomTile;
        if (customTile != null)
        {
            return customTile.isPassable; // 이동 가능 여부 반환
        }
        else
        {
            // CustomTile이 아닌 경우 기본적으로 이동 가능하다고 간주
            return true;
        }
    }

    // 경로를 복원하고, 플레이어를 이동시키는 함수
    private void RetracePath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
    {
        List<Vector3Int> path = new List<Vector3Int>();

        // 목표 지점부터 시작 지점까지 역추적하여 경로 생성
        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }

        // 경로를 반전시켜 시작 지점부터 목표 지점 순서로 변경
        path.Reverse();

        // 경로의 길이를 출력
        Debug.Log($"Path found with {path.Count} steps.");

        // 경로가 존재하면 이동 코루틴 실행
        if (path.Count > 0)
        {
            StartCoroutine(MoveAlongPath(path));
        }
    }

    // 플레이어가 경로를 따라 이동하는 코루틴
    private IEnumerator MoveAlongPath(List<Vector3Int> path)
    {
        foreach (Vector3Int cell in path)
        {
            // 셀의 중심 월드 좌표를 목표 지점으로 설정
            Vector3 worldPos = tilemap.GetCellCenterWorld(cell);

            // 현재 위치에서 목표 지점까지 이동
            while (Vector3.Distance(transform.position, worldPos) > 0.1f)
            {
                // 이동 속도에 따라 위치를 보간하여 이동
                transform.position = Vector3.MoveTowards(transform.position, worldPos, 5f * Time.deltaTime);
                yield return null; // 다음 프레임까지 대기
            }
        }
    }
}
