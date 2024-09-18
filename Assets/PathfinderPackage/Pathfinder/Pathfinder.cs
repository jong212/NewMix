using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinder : MonoBehaviour
{
    // �ִ� �õ� Ƚ�� (��� Ž�� �� ���� ���� ������)
    public int MaxTries = 1000;
    // �밢�� �̵� ��� ����
    public bool AllowDiagonalMovement;

    // �� ��� Ž�� ����� �����ϴ� ����Ʈ
    public List<Vector2> CalculatedLongPath { get; private set; }
    // �� ��� Ž�� �Ϸ� ����
    public bool LongCalculationDone { get; private set; }

    // �־��� ���� ��ǥ���� ��ǥ ��ǥ�� ���� ��θ� ã�� �Լ�
    public List<Vector2> FindPathToGridPoint(Vector2 gridStart, Vector2 gridEnd)
    {
        return FindPath(gridStart, gridEnd); // ���ο��� ��� Ž�� �Լ� ȣ��
    }

    // �� ��θ� ã�� �Լ�, �ڷ�ƾ���� �񵿱� ó���Ͽ� ���� �ð� ���� ����ǵ��� ��
    public void FindLongPathToGridPoint(Vector2 gridStart, Vector2 gridEnd, int speed = 50)
    {
        LongCalculationDone = false; // �� ��� Ž�� �Ϸ� ���� �ʱ�ȭ
        CalculatedLongPath = new List<Vector2>(); // ��� ����Ʈ �ʱ�ȭ

        StartCoroutine(FindLongPathCoroutine(gridStart, gridEnd, PathfindConstants.Directions.None, speed)); // �ڷ�ƾ ����
    }

    // ��� Ž�� �Լ�, ���� �������� ��ǥ �������� ��θ� ã��
    private List<Vector2> FindPath(Vector2 origin, Vector2 target, PathfindConstants.Directions fromDir = PathfindConstants.Directions.None)
    {
        List<pathNode> nodes = new List<pathNode>(); // ��� Ž�� ��� ����Ʈ
        List<Vector2> checkedPositions = new List<Vector2>(); // �̹� üũ�� ��ǥ ����Ʈ
        nodes.Add(new pathNode(Mathf.RoundToInt(origin.x), Mathf.RoundToInt(origin.y), new List<int>(), new List<int>(), fromDir)); // ���� ��� �߰�

        bool foundPath = false; // ��θ� ã�Ҵ��� ����
        List<Vector2> resultPath = null; // ��� ��� ����
        int tries = 0; // �õ� Ƚ��

        // ��θ� ã�Ұų� �õ� Ƚ���� �ʰ��� ������ �ݺ�
        while (!foundPath && tries < MaxTries)
        {
            tries++; // �õ� Ƚ�� ����
            List<pathNode> newNodes = new List<pathNode>(); // ���� �߰��� ��� ����Ʈ

            // ���� Ž�� ���� ��带 ��ȸ
            foreach (pathNode node in nodes)
            {
                // ���� ��� ��ǥ�� üũ���� �ʾ��� ���
                if (!checkedPositions.Contains(new Vector2(node.GridPosX, node.GridPosY)))
                {
                    checkedPositions.Add(new Vector2(node.GridPosX, node.GridPosY)); // üũ�� ��ǥ�� �߰�

                    // ��ǥ ��ǥ�� ���� ������ ��� ��θ� ã�Ҵٰ� �Ǵ�
                    if (Vector2.Distance(new Vector2(node.GridPosX, node.GridPosY), target) < 1)
                    {
                        Vector2[] path = new Vector2[node.GridPathX.Count]; // ��θ� ������ �迭

                        // ��� �迭�� ��������� ��θ� ����
                        for (int i = 0; i < path.Length; i++)
                        {
                            path[i].x = node.GridPathX[i];
                            path[i].y = node.GridPathY[i];
                        }

                        resultPath = new List<Vector2>();
                        resultPath.AddRange(path); // ��� ����Ʈ�� ��ȯ�Ͽ� ����
                        foundPath = true; // ��� Ž�� �Ϸ�
                        print("Found path, " + resultPath.Count + " steps."); // ��� �ܰ� ���

                        break;
                    }

                    // ���� ��尡 ��ġ�� RoomData �������� (���� ��ġ�� �� ����)
                    RoomData room = CellSpawner.Instance.GetRoomAtGridPosition(node.GridPosX, node.GridPosY);

                    if (room != null)
                    {
                        // �� �������� �̵� �������� üũ�ϰ�, ������ ��� ���ο� ��� �߰�
                        if (room.NPossible && node.FromDir != PathfindConstants.Directions.North) // �������� �̵� ����
                        {
                            newNodes.Add(new pathNode(node.GridPosX, node.GridPosY + 1, node.GridPathX, node.GridPathY, PathfindConstants.Directions.South));
                        }
                        if (room.EPossible && node.FromDir != PathfindConstants.Directions.East) // �������� �̵� ����
                        {
                            newNodes.Add(new pathNode(node.GridPosX + 1, node.GridPosY, node.GridPathX, node.GridPathY, PathfindConstants.Directions.West));
                        }
                        if (room.SPossible && node.FromDir != PathfindConstants.Directions.South) // �������� �̵� ����
                        {
                            newNodes.Add(new pathNode(node.GridPosX, node.GridPosY - 1, node.GridPathX, node.GridPathY, PathfindConstants.Directions.North));
                        }
                        if (room.WPossible && node.FromDir != PathfindConstants.Directions.West) // �������� �̵� ����
                        {
                            newNodes.Add(new pathNode(node.GridPosX - 1, node.GridPosY, node.GridPathX, node.GridPathY, PathfindConstants.Directions.East));
                        }

                        // �밢�� �̵� ��� ��, �밢�� �������ε� �̵� ���� ���� Ȯ��
                        if (AllowDiagonalMovement)
                        {
                            if (room.NEPossible && node.FromDir != PathfindConstants.Directions.NorthEast)
                            {
                                newNodes.Add(new pathNode(node.GridPosX + 1, node.GridPosY + 1, node.GridPathX, node.GridPathY, PathfindConstants.Directions.SouthWest));
                            }
                            if (room.NWPossible && node.FromDir != PathfindConstants.Directions.NorthWest)
                            {
                                newNodes.Add(new pathNode(node.GridPosX - 1, node.GridPosY + 1, node.GridPathX, node.GridPathY, PathfindConstants.Directions.SouthEast));
                            }
                            if (room.SEPossible && node.FromDir != PathfindConstants.Directions.SouthEast)
                            {
                                newNodes.Add(new pathNode(node.GridPosX + 1, node.GridPosY - 1, node.GridPathX, node.GridPathY, PathfindConstants.Directions.NorthWest));
                            }
                            if (room.SWPossible && node.FromDir != PathfindConstants.Directions.SouthWest)
                            {
                                newNodes.Add(new pathNode(node.GridPosX - 1, node.GridPosY - 1, node.GridPathX, node.GridPathY, PathfindConstants.Directions.NorthEast));
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("null room"); // �� ������ ���� ��� ���� ���
                    }
                }
            }

            nodes = new List<pathNode>(newNodes); // ���� Ž���� ���� ���ο� ��� ����Ʈ�� ����

            // �õ� Ƚ���� �ִ�ġ�� ������ ��� Ž�� ���з� ����
            if (tries == MaxTries)
                Debug.LogError("Pathfind unsuccessful, " + tries);
        }

        return resultPath; // Ž���� ��� ��ȯ
    }

    // �� ��θ� �񵿱�� Ž���ϴ� �ڷ�ƾ
    private IEnumerator FindLongPathCoroutine(Vector2 origin, Vector2 target, PathfindConstants.Directions fromDir = PathfindConstants.Directions.None, int speed = 50)
    {
        int timer = 0; // ��� �ð� ������

        List<pathNode> nodes = new List<pathNode>(); // Ž�� ���� ��� ����Ʈ
        List<Vector2> checkedPositions = new List<Vector2>(); // �̹� üũ�� ��ǥ ����Ʈ
        nodes.Add(new pathNode(Mathf.RoundToInt(origin.x), Mathf.RoundToInt(origin.y), new List<int>(), new List<int>(), fromDir)); // ���� ��� �߰�

        bool foundPath = false; // ��� Ž�� ����
        List<Vector2> resultPath = null; // ��� ���
        int tries = 0; // �õ� Ƚ��

        // ��θ� ã�Ұų� �õ� Ƚ���� �ʰ��� ������ �ݺ�
        while (!foundPath && tries < MaxTries)
        {
            tries++;
            List<pathNode> newNodes = new List<pathNode>();

            // ��� Ž���� Ÿ�� �̵��� ������ �����ϰ� ����
            foreach (pathNode node in nodes)
            {
                if (!checkedPositions.Contains(new Vector2(node.GridPosX, node.GridPosY)))
                {
                    checkedPositions.Add(new Vector2(node.GridPosX, node.GridPosY));

                    if (Vector2.Distance(new Vector2(node.GridPosX, node.GridPosY), target) < 1)
                    {
                        Vector2[] path = new Vector2[node.GridPathX.Count];
                        for (int i = 0; i < path.Length; i++)
                        {
                            path[i].x = node.GridPathX[i];
                            path[i].y = node.GridPathY[i];
                        }

                        resultPath = new List<Vector2>();
                        resultPath.AddRange(path);
                        foundPath = true;
                        print("Found path, " + resultPath.Count + " steps.");
                        break;
                    }

                    RoomData room = CellSpawner.Instance.GetRoomAtGridPosition(node.GridPosX, node.GridPosY);

                    if (room != null)
                    {
                        // ���� ������ ��� ������ ������ �������� ��� �߰�
                        if (room.NPossible && node.FromDir != PathfindConstants.Directions.North)
                        {
                            newNodes.Add(new pathNode(node.GridPosX, node.GridPosY + 1, node.GridPathX, node.GridPathY, PathfindConstants.Directions.South));
                        }
                        if (room.EPossible && node.FromDir != PathfindConstants.Directions.East)
                        {
                            newNodes.Add(new pathNode(node.GridPosX + 1, node.GridPosY, node.GridPathX, node.GridPathY, PathfindConstants.Directions.West));
                        }
                        if (room.SPossible && node.FromDir != PathfindConstants.Directions.South)
                        {
                            newNodes.Add(new pathNode(node.GridPosX, node.GridPosY - 1, node.GridPathX, node.GridPathY, PathfindConstants.Directions.North));
                        }
                        if (room.WPossible && node.FromDir != PathfindConstants.Directions.West)
                        {
                            newNodes.Add(new pathNode(node.GridPosX - 1, node.GridPosY, node.GridPathX, node.GridPathY, PathfindConstants.Directions.East));
                        }

                        // �밢�� �̵� ��� ���� ó��
                        if (AllowDiagonalMovement)
                        {
                            if (room.NEPossible && node.FromDir != PathfindConstants.Directions.NorthEast)
                            {
                                newNodes.Add(new pathNode(node.GridPosX + 1, node.GridPosY + 1, node.GridPathX, node.GridPathY, PathfindConstants.Directions.SouthWest));
                            }
                            if (room.NWPossible && node.FromDir != PathfindConstants.Directions.NorthWest)
                            {
                                newNodes.Add(new pathNode(node.GridPosX - 1, node.GridPosY + 1, node.GridPathX, node.GridPathY, PathfindConstants.Directions.SouthEast));
                            }
                            if (room.SEPossible && node.FromDir != PathfindConstants.Directions.SouthEast)
                            {
                                newNodes.Add(new pathNode(node.GridPosX + 1, node.GridPosY - 1, node.GridPathX, node.GridPathY, PathfindConstants.Directions.NorthWest));
                            }
                            if (room.SWPossible && node.FromDir != PathfindConstants.Directions.SouthWest)
                            {
                                newNodes.Add(new pathNode(node.GridPosX - 1, node.GridPosY - 1, node.GridPathX, node.GridPathY, PathfindConstants.Directions.NorthEast));
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("null room");
                    }
                }

                timer++; // �ð� ����

                // ���� �ð� ���� Ž�������� ���� ���������� �ѱ�
                if (timer > speed)
                {
                    timer = 0;
                    yield return null;
                }
            }

            nodes = new List<pathNode>(newNodes); // ���ο� ���� ����

            if (tries == MaxTries)
                Debug.LogError("Pathfind unsuccessful, " + tries);

            yield return null;
        }

        CalculatedLongPath = resultPath; // Ž���� �� ��� ����
        LongCalculationDone = true; // ��� Ž�� �Ϸ�
    }
}

// ��� Ž���� ���Ǵ� ��� ����ü
public struct pathNode
{
    public pathNode(int x, int y, List<int> xHist, List<int> yHist, PathfindConstants.Directions fromDir = PathfindConstants.Directions.None)
    {
        FromDir = fromDir; // ���� ��忡 ���� ����
        GridPosX = x; // ����� x ��ǥ
        GridPosY = y; // ����� y ��ǥ

        GridPathX = new List<int>(xHist); // ����� x ��ǥ ���
        GridPathY = new List<int>(yHist); // ����� y ��ǥ ���

        GridPathX.Add(x); // ���� ����� ��ǥ �߰�
        GridPathY.Add(y);
    }

    public PathfindConstants.Directions FromDir; // ���� (��� Ž���� �ʿ��� ����)
    public int GridPosX; // ����� x ��ǥ
    public int GridPosY; // ����� y ��ǥ
    public List<int> GridPathX; // ��λ��� x ��ǥ ����Ʈ
    public List<int> GridPathY; // ��λ��� y ��ǥ ����Ʈ
}
