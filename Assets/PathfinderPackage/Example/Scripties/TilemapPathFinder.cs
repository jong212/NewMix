using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TilemapPathFinder : MonoBehaviour
{
    // ��� Ž���� ����� Ÿ�ϸ� (����Ƽ �����Ϳ��� �Ҵ�)
    public Tilemap tilemap;
    // Pathfinder ��ũ��Ʈ ���� (����Ƽ �����Ϳ��� �Ҵ�)
    public Pathfinder pathfinder;

    void Update()
    {
        // ���콺 ���� ��ư�� Ŭ���Ǿ��� �� ����
        if (Input.GetMouseButtonDown(0))
        {
            // ���콺 Ŭ�� ��ġ�� ���� ��ǥ�� ��ȯ
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -Camera.main.transform.position.z; // ī�޶� -Z ��ġ�� ���� ��� ����� ��ȯ

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            worldPos.z = 0; // Z���� 0���� �����Ͽ� 2D ��鿡���� ���

            // ���� ��ǥ�� Ÿ�ϸ��� �� ��ǥ�� ��ȯ
            Vector3Int cellPos = tilemap.WorldToCell(worldPos);

            // Ŭ���� ���� ��ǥ�� ���
            Debug.Log($"Clicked Cell Position: {cellPos}");

            // Ŭ���� ���� Ÿ���� �ִ��� Ȯ��
            if (tilemap.HasTile(cellPos))
            {
                Debug.Log("Tile exists at the clicked position.");

                // �÷��̾��� ���� ��ġ�� ��ǥ ��ġ ����
                Vector3 startPos = pathfinder.transform.position;
                Vector3 targetPos = tilemap.GetCellCenterWorld(cellPos); // Ÿ���� �߽� ��ǥ ���

                // ��� Ž�� �� �̵� ����
                pathfinder.FindPath(startPos, targetPos);
            }
            else
            {
                Debug.Log("No tile found at the clicked position.");
            }
        }
    }
}
