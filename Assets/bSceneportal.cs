using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bSceneportal : MonoBehaviour
{
    [SerializeField] LayerMask targetLayerMask;
    // Start is called before the first frame update
    private void OnCollisionEnter(Collision collision)
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((targetLayerMask & (1 << other.gameObject.layer)) != 0)
        {
            BackendGameData.Instance.userData.LastMap = "B";
            StaticManager.Instance.ConfirmleaveSessionHook();
        }
    }
}
