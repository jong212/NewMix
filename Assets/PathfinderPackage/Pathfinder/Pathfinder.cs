using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinder : MonoBehaviour
{
    // ��� Ž���� ����� Ÿ�ϸ� (����Ƽ �����Ϳ��� �Ҵ�)
    public Tilemap tilemap;

    // A* �˰����� ����Ͽ� ��θ� ã�� �Լ�
    public void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        // ���� ��ġ�� ��ǥ ��ġ�� Ÿ�ϸ��� �� ��ǥ�� ��ȯ
        Vector3Int startCell = tilemap.WorldToCell(startPos);
        Vector3Int targetCell = tilemap.WorldToCell(targetPos);

        // Z�� ��ǥ�� 0���� �����Ͽ� 2D ��鿡�� ���ǵ��� ��
        startCell.z = 0;
        targetCell.z = 0;

        // ���� ���� ��ǥ �� ������ �α׷� ���
        Debug.Log($"Start Cell: {startCell}, Target Cell: {targetCell}");

        // ��ǥ ���� Ÿ���� ���ų�, �̵� �Ұ����� Ÿ���̸� ��� Ž�� ����
        if (!tilemap.HasTile(targetCell) || !IsTilePassable(tilemap.GetTile(targetCell)))
        {
            Debug.Log("No tile found at target cell or tile is impassable, stopping pathfinding.");
            return;
        }

        // A* �˰��� �ʿ��� ������ ���� �ʱ�ȭ
        List<Vector3Int> openList = new List<Vector3Int>(); // Ž���� �� ���
        HashSet<Vector3Int> closedList = new HashSet<Vector3Int>(); // �̹� Ž���� �� ���
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>(); // �� ���� �����ϱ� ������ ��
        Dictionary<Vector3Int, float> gScore = new Dictionary<Vector3Int, float>(); // ���� �������� �ش� �������� ���� ���
        Dictionary<Vector3Int, float> fScore = new Dictionary<Vector3Int, float>(); // gScore + �޸���ƽ ���� ���

        // ���� ���� openList�� �߰��ϰ�, gScore�� fScore �ʱ�ȭ
        openList.Add(startCell);
        gScore[startCell] = 0; // ���� ������ gScore�� 0
        fScore[startCell] = HeuristicCostEstimate(startCell, targetCell); // fScore�� �޸���ƽ ���������� �ʱ�ȭ

        // openList�� Ž���� ���� �����ִ� ���� �ݺ�
        while (openList.Count > 0)
        {
            // fScore�� ���� ���� ���� �����Ͽ� ���� ���� ����
            Vector3Int current = GetLowestFScore(openList, fScore);

            // ���� ���� ��ǥ ���� �������� Ȯ��
            if (current.x == targetCell.x && current.y == targetCell.y)
            {
                Debug.Log("Retracing path to move to target.");
                // ��θ� �����ϰ� �̵� ����
                RetracePath(cameFrom, current);
                return;
            }

            // ���� ���� openList���� �����ϰ� closedList�� �߰�
            openList.Remove(current);
            closedList.Add(current);

            // ���� ���� �̿� ������ �˻�
            foreach (Vector3Int neighbor in GetNeighbors(current))
            {
                // �̹� Ž���� ���� ����
                if (closedList.Contains(neighbor))
                    continue;

                // �̿� ���� Ÿ�� ��������
                TileBase tile = tilemap.GetTile(neighbor);

                // Ÿ���� ���ų�, �̵� �Ұ����� Ÿ���̸� ����
                if (tile == null || !IsTilePassable(tile))
                {
                    Debug.Log($"Skipping neighbor: {neighbor}, tile is impassable.");
                    continue;
                }

                // ���� ���� ���� �̿� ���� ���� ��� ��� (���⼭�� ����ġ�� ��� 1�� ����)
                float tentativeGScore = gScore[current] + 1;

                // �̿� ���� openList�� ���ų�, �� ª�� ��θ� �߰��� ���
                if (!openList.Contains(neighbor) || tentativeGScore < gScore.GetValueOrDefault(neighbor, Mathf.Infinity))
                {
                    // �̿� ���� ���� �ִ� ��� ����
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, targetCell);

                    // openList�� ������ �߰�
                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        // ��θ� ã�� ���� ���
        Debug.Log("No valid path found.");
    }

    // �޸���ƽ �Լ�: ���� ������ ��ǥ �������� ���� ��� ��� (����ư �Ÿ� ���)
    private float HeuristicCostEstimate(Vector3Int start, Vector3Int goal)
    {
        return Mathf.Abs(goal.x - start.x) + Mathf.Abs(goal.y - start.y);
    }

    // fScore�� ���� ���� ���� openList���� �����ϴ� �Լ�
    private Vector3Int GetLowestFScore(List<Vector3Int> openList, Dictionary<Vector3Int, float> fScore)
    {
        Vector3Int lowest = openList[0];
        float lowestScore = fScore[lowest];

        // openList�� �� ���� ���� fScore�� ���Ͽ� �ּҰ� ã��
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

    // ���� ���� ������ �̿� ������ ����Ʈ�� ��ȯ�ϴ� �Լ�
    private List<Vector3Int> GetNeighbors(Vector3Int cell)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>();

        // �����¿� �̿� �߰�
        neighbors.Add(new Vector3Int(cell.x + 1, cell.y, cell.z)); // ������
        neighbors.Add(new Vector3Int(cell.x - 1, cell.y, cell.z)); // ����
        neighbors.Add(new Vector3Int(cell.x, cell.y + 1, cell.z)); // ����
        neighbors.Add(new Vector3Int(cell.x, cell.y - 1, cell.z)); // �Ʒ���

        // �밢�� �̿� �߰� (�ʿ��� ���)
        neighbors.Add(new Vector3Int(cell.x + 1, cell.y + 1, cell.z)); // ������ ��
        neighbors.Add(new Vector3Int(cell.x - 1, cell.y + 1, cell.z)); // ���� ��
        neighbors.Add(new Vector3Int(cell.x + 1, cell.y - 1, cell.z)); // ������ �Ʒ�
        neighbors.Add(new Vector3Int(cell.x - 1, cell.y - 1, cell.z)); // ���� �Ʒ�

        return neighbors;
    }

    // Ÿ���� �̵� ���� ���θ� Ȯ���ϴ� �Լ�
    private bool IsTilePassable(TileBase tile)
    {
        // Ÿ���� CustomTile�� ĳ�����Ͽ� isPassable �Ӽ� Ȯ��
        CustomTile customTile = tile as CustomTile;
        if (customTile != null)
        {
            return customTile.isPassable; // �̵� ���� ���� ��ȯ
        }
        else
        {
            // CustomTile�� �ƴ� ��� �⺻������ �̵� �����ϴٰ� ����
            return true;
        }
    }

    // ��θ� �����ϰ�, �÷��̾ �̵���Ű�� �Լ�
    private void RetracePath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
    {
        List<Vector3Int> path = new List<Vector3Int>();

        // ��ǥ �������� ���� �������� �������Ͽ� ��� ����
        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }

        // ��θ� �������� ���� �������� ��ǥ ���� ������ ����
        path.Reverse();

        // ����� ���̸� ���
        Debug.Log($"Path found with {path.Count} steps.");

        // ��ΰ� �����ϸ� �̵� �ڷ�ƾ ����
        if (path.Count > 0)
        {
            StartCoroutine(MoveAlongPath(path));
        }
    }

    // �÷��̾ ��θ� ���� �̵��ϴ� �ڷ�ƾ
    private IEnumerator MoveAlongPath(List<Vector3Int> path)
    {
        foreach (Vector3Int cell in path)
        {
            // ���� �߽� ���� ��ǥ�� ��ǥ �������� ����
            Vector3 worldPos = tilemap.GetCellCenterWorld(cell);

            // ���� ��ġ���� ��ǥ �������� �̵�
            while (Vector3.Distance(transform.position, worldPos) > 0.1f)
            {
                // �̵� �ӵ��� ���� ��ġ�� �����Ͽ� �̵�
                transform.position = Vector3.MoveTowards(transform.position, worldPos, 5f * Time.deltaTime);
                yield return null; // ���� �����ӱ��� ���
            }
        }
    }
}
