using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tester : MonoBehaviour
{
    private Pathfinder pathFinder; // 경로 탐색을 담당하는 Pathfinder 클래스
    private List<Vector2> movement; // 이동할 경로를 저장하는 리스트
    public static Tester Instance { get; private set; } // Singleton 패턴을 위한 Instance 변수
    private List<Tile> tiles; // 모든 타일을 저장하는 리스트

    private Vector2 lastTargetPos = Vector2.negativeInfinity; // 마지막 클릭한 타일의 좌표 저장
    private Vector2 newTargetPos = Vector2.negativeInfinity; // 새롭게 클릭된 타일의 좌표 저장
    private bool isMoving = false; // 플레이어가 현재 이동 중인지 여부
    private bool newTargetRequested = false; // 새로운 경로 요청 여부 저장
    private bool targetReached = false; // 현재 타일에 도착했는지 여부
    private float speed = 10; // 플레이어 이동 속도
    [SerializeField] private GUISkin skin; // GUI 스킨 설정

    private bool blinked = true; // 타일 깜빡임 여부 (깜빡임이 끝난 후 이동 가능)

    void Awake()
    {
        Instance = this; // Singleton 설정
    }

    void OnGUI()
    {
        GUI.skin = skin; // GUI 스킨 적용

        // 슬라이더를 통해 이동 속도 설정
        speed = GUI.HorizontalSlider(new Rect(0, 0, Screen.width * 0.3f, 16), speed, 0.1f, 20);

        // 대각선 이동 허용 여부를 토글 버튼으로 설정
        pathFinder.AllowDiagonalMovement = GUI.Toggle(new Rect(0, Screen.height * 0.05f, Screen.width * 0.2f, Screen.height * 0.04f), pathFinder.AllowDiagonalMovement, "Diagonal Movement");

        // 모든 타일 활성화 버튼
        if (GUI.Button(new Rect(Screen.width * 0.2f, Screen.height * 0.02f, Screen.width * 0.1f, Screen.height * 0.04f), "Activate All"))
        {
            foreach (Tile tile in tiles)
                tile.SetActive(true); // 타일 활성화
        }

        // 모든 타일 비활성화 버튼 (플레이어가 서 있는 타일은 제외)
        if (GUI.Button(new Rect(Screen.width * 0.2f, Screen.height * 0.06f, Screen.width * 0.1f, Screen.height * 0.04f), "Deactivate All"))
        {
            foreach (Tile tile in tiles)
                tile.SetActive(false); // 타일 비활성화

            // 플레이어가 서 있는 타일은 활성화 상태 유지
            tiles.Find(o => o.gridPos == PathfindConstants.WorldToGrid(transform.position)).SetActive(true);
        }
    }

    void Start()
    {
        // 씬에 있는 모든 타일을 찾고 tiles 리스트에 추가
        tiles = new List<Tile>();
        tiles.AddRange((Tile[])GameObject.FindObjectsOfType(typeof(Tile)));

        // Pathfinder 컴포넌트 가져오기
        pathFinder = GetComponent<Pathfinder>();
        movement = new List<Vector2>(); // 경로 리스트 초기화
    }

    public void BlinkAt(Vector2 pos)
    {
        // 특정 위치의 타일을 파란색으로 깜빡이게 설정
        tiles.Find(o => o.gridPos == pos).Blink();
    }

    // 이동할 경로의 타일들을 깜빡이게 하는 코루틴
    IEnumerator BlinkPath()
    {
        int i = 0;
        Vector2[] moves = movement.ToArray(); // 이동 경로를 배열로 복사

        // 이동 경로에 있는 타일을 차례대로 깜빡이게 함
        while (i < moves.Length)
        {
            tiles.Find(o => o.gridPos == moves[i]).Blink(); // 해당 타일을 파란색으로 깜빡이게 함
            i++;
            yield return new WaitForSeconds(0.03f); // 짧은 지연 후 다음 타일 깜빡임
        }

        blinked = true; // 모든 타일이 깜빡인 후 이동 가능 상태로 전환
    }

    void Update()
    {
        // 경로가 있고, 타일이 깜빡임을 완료했고, 이동 중일 때 실행
        if (movement != null && movement.Count > 0 && blinked && isMoving)
        {
            // 이동할 다음 타일의 월드 좌표를 구함
            Vector3 target = PathfindConstants.GridToWorld((int)movement[0].x, (int)movement[0].y);
            target.z = target.y;
            target.y = 1; // 타일의 높이를 설정 (임의로 1로 설정)

            // 플레이어가 목표 타일을 바라보게 함
            transform.LookAt(target);

            // 플레이어를 목표 타일 방향으로 이동
            transform.position += transform.forward * Mathf.Clamp(speed * Time.deltaTime, 0, Vector3.Distance(transform.position, target));

            // 목표 타일에 거의 도착했을 때
            if (Vector3.Distance(transform.position, target) < 0.1f)
            {
                targetReached = true; // 목표 타일에 도착했다고 표시

                movement.RemoveAt(0); // 현재 타일을 경로 리스트에서 제거

                // 새로운 경로 요청이 있을 경우 처리
                if (newTargetRequested)
                {
                    lastTargetPos = newTargetPos; // 새로운 목표를 저장
                    StartCoroutine(MoveTo(newTargetPos, false)); // 새로운 경로로 이동
                    newTargetRequested = false; // 새로운 경로 요청 초기화
                }

                // 경로가 완료되었을 때 이동 종료
                if (movement.Count == 0)
                {
                    isMoving = false; // 이동 완료
                }
            }
        }
    }

    // 특정 타일로 이동을 요청하는 함수
    public void MoveToPos(Vector2 gridPos, bool longPath = false)
    {
        // 현재 목표와 새로운 목표가 같으면 경로 탐색을 무시
        if (gridPos == lastTargetPos)
        {
            Debug.Log("Same target as the last one, skipping pathfinding.");
            return; // 같은 경로이므로 return
        }

        // 플레이어가 아직 목표 타일에 도착하지 않았을 때
        if (isMoving && !targetReached)
        {
            newTargetRequested = true; // 새로운 경로 요청 저장
            newTargetPos = gridPos; // 새로운 목표 타일 저장
            Debug.Log("New target requested. Waiting for current tile to be reached.");
        }
        else if (isMoving && targetReached) // 타일에 거의 도착한 상태에서 클릭했을 때
        {
            movement.Clear(); // 현재 이동 경로 취소
            lastTargetPos = gridPos; // 새로운 목표 타일 설정
            StartCoroutine(MoveTo(gridPos, longPath)); // 새로운 목표로 이동
            targetReached = false; // 타일 도착 여부 초기화
        }
        else
        {
            // 이동 중이 아니면 즉시 경로 설정
            lastTargetPos = gridPos;
            StartCoroutine(MoveTo(gridPos, longPath));
        }
    }

    // 특정 좌표로 이동하는 코루틴 함수
    private IEnumerator MoveTo(Vector2 gridPos, bool longPath)
    {
        isMoving = true; // 이동 시작

        // 경로 탐색 (기본 경로 또는 긴 경로 탐색)
        if (!longPath)
        {
            Debug.Log("Finding Path");
            movement = pathFinder.FindPathToGridPoint(PathfindConstants.WorldToGrid(transform.position), gridPos); // 경로 탐색
        }
        else
        {
            Debug.Log("Finding Long Path");
            pathFinder.FindLongPathToGridPoint(PathfindConstants.WorldToGrid(transform.position), gridPos);
            while (!pathFinder.LongCalculationDone)
                yield return null; // 긴 경로 계산이 완료될 때까지 대기
            movement = pathFinder.CalculatedLongPath;
        }

        // 유효한 경로가 없을 경우 이동 종료
        if (movement == null || movement.Count == 0)
        {
            Debug.LogError("No valid path found!");
            isMoving = false;
            yield break;
        }

        StartCoroutine(BlinkPath()); // 타일 깜빡임 코루틴 시작
        blinked = true; // 깜빡임 완료 후 이동 가능 상태로 전환
        yield return null;
    }
}
