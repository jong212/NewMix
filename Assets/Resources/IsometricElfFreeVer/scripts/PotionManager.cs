using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionManager : MonoBehaviour
{
    Animator animator;
    float speed = 0.2f;
    Rigidbody2D rb;
    public float leftTime;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.up * speed;
        Destroy(gameObject, leftTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
