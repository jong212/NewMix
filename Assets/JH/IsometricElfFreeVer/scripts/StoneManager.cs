using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneManager : MonoBehaviour
{
    private float speed = 2.5f;
   [SerializeField] Transform target;

    public float timer = 1f;

    void Update()// オブジェクトを目標点まで移動させる
    {
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        Destroy(gameObject, timer);
    }
    
}

