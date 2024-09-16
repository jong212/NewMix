using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class IconCycler : MonoBehaviour
{
    public Sprite[] sprites;

    public Image image;
    public float cycleTime = 1f;
    float t = 0;

    private void LateUpdate()
    {
        t+=Time.deltaTime;
        if(t>=cycleTime)
        {
            SetRandomIcon();
            t = 0;
        }
    }

    public void SetRandomIcon()
    {
        CycleIcon(Random.Range(0,sprites.Length));
    }

    public void CycleIcon(int index)
    {
        image.sprite = sprites[index];
    }
}
