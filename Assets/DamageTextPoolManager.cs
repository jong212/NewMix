using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTextPoolManager : MonoBehaviour
{
    public GameObject damageTextPrefab;
    public int poolSize = 60;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Start()
    {
        // 풀 생성
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(damageTextPrefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public void ShowDamage(NetworkObject trs, string damage)
    {
        
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.transform.position = trs.gameObject.transform.position;
            obj.SetActive(true);

            // 텍스트 초기화
            DamageText damageText = obj.GetComponent<DamageText>();
            damageText.Initialize(trs.gameObject.transform,damage);
        }
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
