using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Custom Tile", menuName = "Tiles/Custom Tile")]
public class CustomTile : Tile
{
    public bool isPassable = true; // �⺻���� true�� ����
}
