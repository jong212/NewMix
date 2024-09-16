using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WorldUI : MonoBehaviour
{
    public virtual void LateUpdate()
    {
        transform.forward = Camera.main.transform.forward;
    }

    public virtual void DestroySelf(float length)
    {
        StartCoroutine(WaitAndDestroy(length));
    }

    public virtual IEnumerator WaitAndDestroy(float length)
    {
        
        yield return new WaitForSeconds(length);
        Destroy(gameObject);
    }
}
