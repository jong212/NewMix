using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class PoolTest : MonoBehaviour
{
    void Start()
    {
        CoroutineGo();
    }

    private void CoroutineGo()
    {
        StartCoroutine(AddObject());
    }

    private IEnumerator AddObject()
    {
        var countLenth = 0;
        while (countLenth < 15)
        {
            GameObject obj = PoolManager.Instance.GetPoolObject(PoolObjectType.Mon1);

            yield return new WaitForSeconds(1f);                                        


            StartCoroutine(CoolObject(obj, PoolObjectType.Mon1));
            countLenth++;
        }
    }

    private IEnumerator CoolObject(GameObject obj, PoolObjectType type)
    {
        yield return new WaitForSeconds(5f);                                        
        if (obj != null && obj.activeInHierarchy)
        {
            PoolManager.Instance.CoolObject(obj, type);                   
        }
    }
}
