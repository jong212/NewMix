using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropItemPrefab : MonoBehaviour
{
    public Text ItemText;
    public PlayerRef PlayerRef;

    private Text textMesh;
    private RectTransform rectTransform;
    private float lifetime = 1f; // 텍스트가 사라지는 시간

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        textMesh = GetComponent<Text>();
    }

}
