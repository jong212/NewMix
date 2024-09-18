using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TilemapPathFinder : MonoBehaviour
{
    // 경로 탐색에 사용할 타일맵 (유니티 에디터에서 할당)
    public Tilemap tilemap;
    // Pathfinder 스크립트 참조 (유니티 에디터에서 할당)
    public Pathfinder pathfinder;

    void Update()
    {
        // 마우스 왼쪽 버튼이 클릭되었을 때 실행
        if (Input.GetMouseButtonDown(0))
        {
            // 마우스 클릭 위치를 월드 좌표로 변환
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -Camera.main.transform.position.z; // 카메라가 -Z 위치에 있을 경우 양수로 변환

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            worldPos.z = 0; // Z축을 0으로 설정하여 2D 평면에서만 계산

            // 월드 좌표를 타일맵의 셀 좌표로 변환
            Vector3Int cellPos = tilemap.WorldToCell(worldPos);

            // 클릭한 셀의 좌표를 출력
            Debug.Log($"Clicked Cell Position: {cellPos}");

            // 클릭한 셀에 타일이 있는지 확인
            if (tilemap.HasTile(cellPos))
            {
                Debug.Log("Tile exists at the clicked position.");

                // 플레이어의 현재 위치와 목표 위치 설정
                Vector3 startPos = pathfinder.transform.position;
                Vector3 targetPos = tilemap.GetCellCenterWorld(cellPos); // 타일의 중심 좌표 사용

                // 경로 탐색 및 이동 실행
                pathfinder.FindPath(startPos, targetPos);
            }
            else
            {
                Debug.Log("No tile found at the clicked position.");
            }
        }
    }
}
