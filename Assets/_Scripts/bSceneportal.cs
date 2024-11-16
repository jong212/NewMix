using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum EselectMapName { A, B }
public class bSceneportal : MonoBehaviour
{
    [SerializeField] LayerMask _targetLayerMask;
    [SerializeField] EselectMapName _selectMapName;
    // Start is called before the first frame update
    private void OnCollisionEnter(Collision collision)
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
      
        if ((_targetLayerMask & (1 << other.gameObject.layer)) != 0)
        {

            BackendGameData.Instance.userData.LastMap = _selectMapName.ToString();
            StaticManager.Instance.ConfirmleaveSessionHook();
        }
    }
}
