using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ScalePulse : MonoBehaviour
{
    private Image image;
    public float scalingTime, scalingSpeed = 0.3f, targetScale = 2.5f;
    public float length = 0.3f;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        scalingTime = Time.time * scalingSpeed;
        image.rectTransform.localScale = new Vector3(
            Mathf.PingPong(scalingTime, length) + targetScale,
            Mathf.PingPong(scalingTime, length) + targetScale, 1
        );
    }
}
