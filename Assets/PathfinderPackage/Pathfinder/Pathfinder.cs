using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinder : MonoBehaviour
{
    // 최대 시도 횟수 (경로 탐색 시 무한 루프 방지용)
    public int MaxTries = 1000;
    // 대각선 이동 허용 여부
    public bool AllowDiagonalMovement;

    // 긴 경로 탐색 결과를 저장하는 리스트
    public List<Vector2> CalculatedLongPath { get; private set; }
    // 긴 경로 탐색 완료 여부
    public bool LongCalculationDone { get; private set; }

    // 주어진 시작 좌표에서 목표 좌표로 가는 경로를 찾는 함수
    public List<Vector2> FindPathToGridPoint(Vector2 gridStart, Vector2 gridEnd)
    {
        return FindPath(gridStart, gridEnd); // 내부에서 경로 탐색 함수 호출
    }

    // 긴 경로를 찾는 함수, 코루틴으로 비동기 처리하여 일정 시간 동안 실행되도록 함
    public void FindLongPathToGridPoint(Vector2 gridStart, Vector2 gridEnd, int speed = 50)
    {
        LongCalculationDone = false; // 긴 경로 탐색 완료 여부 초기화
        CalculatedLongPath = new List<Vector2>(); // 경로 리스트 초기화

        StartCoroutine(FindLongPathCoroutine(gridStart, gridEnd, PathfindConstants.Directions.None, speed)); // 코루틴 시작
    }

    // 경로 탐색 함수, 시작 지점에서 목표 지점까지 경로를 찾음
    private List<Vector2> FindPath(Vector2 origin, Vector2 target, PathfindConstants.Directions fromDir = PathfindConstants.Directions.None)
    {
        List<pathNode> nodes = new List<pathNode>(); // 경로 탐색 노드 리스트
        List<Vector2> checkedPositions = new List<Vector2>(); // 이미 체크한 좌표 리스트
        nodes.Add(new pathNode(Mathf.RoundToInt(origin.x), Mathf.RoundToInt(origin.y), new List<int>(), new List<int>(), fromDir)); // 시작 노드 추가

        bool foundPath = false; // 경로를 찾았는지 여부
        List<Vector2> resultPath = null; // 결과 경로 저장
        int tries = 0; // 시도 횟수

        // 경로를 찾았거나 시도 횟수를 초과할 때까지 반복
        while (!foundPath && tries < MaxTries)
        {
            tries++; // 시도 횟수 증가
            List<pathNode> newNodes = new List<pathNode>(); // 새로 추가될 노드 리스트

            // 현재 탐색 중인 노드를 순회
            foreach (pathNode node in nodes)
            {
                // 현재 노드 좌표가 체크되지 않았을 경우
                if (!checkedPositions.Contains(new Vector2(node.GridPosX, node.GridPosY)))
                {
                    checkedPositions.Add(new Vector2(node.GridPosX, node.GridPosY)); // 체크한 좌표로 추가

                    // 목표 좌표에 거의 도착한 경우 경로를 찾았다고 판단
                    if (Vector2.Distance(new Vector2(node.GridPosX, node.GridPosY), target) < 1)
                    {
                        Vector2[] path = new Vector2[node.GridPathX.Count]; // 경로를 저장할 배열

                        // 경로 배열에 현재까지의 경로를 저장
                        for (int i = 0; i < path.Length; i++)
                        {
                            path[i].x = node.GridPathX[i];
                            path[i].y = node.GridPathY[i];
                        }

                        resultPath = new List<Vector2>();
                        resultPath.AddRange(path); // 경로 리스트로 변환하여 저장
                        foundPath = true; // 경로 탐색 완료
                        print("Found path, " + resultPath.Count + " steps."); // 경로 단계 출력

                        break;
                    }

                    // 현재 노드가 위치한 RoomData 가져오기 (현재 위치의 방 정보)
                    RoomData room = CellSpawner.Instance.GetRoomAtGridPosition(node.GridPosX, node.GridPosY);

                    if (room != null)
                    {
                        // 각 방향으로 이동 가능한지 체크하고, 가능한 경우 새로운 노드 추가
                        if (room.NPossible && node.FromDir != PathfindConstants.Directions.North) // 북쪽으로 이동 가능
                        {
                            newNodes.Add(new pathNode(node.GridPosX, node.GridPosY + 1, node.GridPathX, node.GridPathY, PathfindConstants.Directions.South));
                        }
                        if (room.EPossible && node.FromDir != PathfindConstants.Directions.East) // 동쪽으로 이동 가능
                        {
                            newNodes.Add(new pathNode(node.GridPosX + 1, node.GridPosY, node.GridPathX, node.GridPathY, PathfindConstants.Directions.West));
                        }
                        if (room.SPossible && node.FromDir != PathfindConstants.Directions.South) // 남쪽으로 이동 가능
                        {
                            newNodes.Add(new pathNode(node.GridPosX, node.GridPosY - 1, node.GridPathX, node.GridPathY, PathfindConstants.Directions.North));
                        }
                        if (room.WPossible && node.FromDir != PathfindConstants.Directions.West) // 서쪽으로 이동 가능
                        {
                            newNodes.Add(new pathNode(node.GridPosX - 1, node.GridPosY, node.GridPathX, node.GridPathY, PathfindConstants.Directions.East));
                        }

                        // 대각선 이동 허용 시, 대각선 방향으로도 이동 가능 여부 확인
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
                        Debug.LogError("null room"); // 방 정보가 없을 경우 에러 출력
                    }
                }
            }

            nodes = new List<pathNode>(newNodes); // 다음 탐색을 위해 새로운 노드 리스트로 갱신

            // 시도 횟수가 최대치를 넘으면 경로 탐색 실패로 간주
            if (tries == MaxTries)
                Debug.LogError("Pathfind unsuccessful, " + tries);
        }

        return resultPath; // 탐색된 경로 반환
    }

    // 긴 경로를 비동기로 탐색하는 코루틴
    private IEnumerator FindLongPathCoroutine(Vector2 origin, Vector2 target, PathfindConstants.Directions fromDir = PathfindConstants.Directions.None, int speed = 50)
    {
        int timer = 0; // 경과 시간 측정용

        List<pathNode> nodes = new List<pathNode>(); // 탐색 중인 노드 리스트
        List<Vector2> checkedPositions = new List<Vector2>(); // 이미 체크한 좌표 리스트
        nodes.Add(new pathNode(Mathf.RoundToInt(origin.x), Mathf.RoundToInt(origin.y), new List<int>(), new List<int>(), fromDir)); // 시작 노드 추가

        bool foundPath = false; // 경로 탐색 여부
        List<Vector2> resultPath = null; // 결과 경로
        int tries = 0; // 시도 횟수

        // 경로를 찾았거나 시도 횟수를 초과할 때까지 반복
        while (!foundPath && tries < MaxTries)
        {
            tries++;
            List<pathNode> newNodes = new List<pathNode>();

            // 경로 탐색과 타일 이동은 이전과 동일하게 동작
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
                        // 방이 존재할 경우 위에서 설명한 방향으로 노드 추가
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

                        // 대각선 이동 허용 여부 처리
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

                timer++; // 시간 증가

                // 일정 시간 동안 탐색했으면 다음 프레임으로 넘김
                if (timer > speed)
                {
                    timer = 0;
                    yield return null;
                }
            }

            nodes = new List<pathNode>(newNodes); // 새로운 노드로 갱신

            if (tries == MaxTries)
                Debug.LogError("Pathfind unsuccessful, " + tries);

            yield return null;
        }

        CalculatedLongPath = resultPath; // 탐색된 긴 경로 저장
        LongCalculationDone = true; // 경로 탐색 완료
    }
}

// 경로 탐색에 사용되는 노드 구조체
public struct pathNode
{
    public pathNode(int x, int y, List<int> xHist, List<int> yHist, PathfindConstants.Directions fromDir = PathfindConstants.Directions.None)
    {
        FromDir = fromDir; // 현재 노드에 들어온 방향
        GridPosX = x; // 노드의 x 좌표
        GridPosY = y; // 노드의 y 좌표

        GridPathX = new List<int>(xHist); // 경로의 x 좌표 기록
        GridPathY = new List<int>(yHist); // 경로의 y 좌표 기록

        GridPathX.Add(x); // 현재 노드의 좌표 추가
        GridPathY.Add(y);
    }

    public PathfindConstants.Directions FromDir; // 방향 (경로 탐색에 필요한 정보)
    public int GridPosX; // 노드의 x 좌표
    public int GridPosY; // 노드의 y 좌표
    public List<int> GridPathX; // 경로상의 x 좌표 리스트
    public List<int> GridPathY; // 경로상의 y 좌표 리스트
}
