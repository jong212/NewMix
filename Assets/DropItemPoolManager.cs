using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItemPoolManager : MonoBehaviour
{
    [SerializeField] private GameObject _dropItemPrefab;
    public int poolSize = 60;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Start()
    {
        // 풀 생성
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(_dropItemPrefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public void ShowDropItem(NetworkObject trs, float ItemId,string Nickname,int dropIdx)
    {
        
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.transform.position = trs.gameObject.transform.position;
            obj.SetActive(true);

            // 텍스트 초기화
            DropItemPrefab damageText = obj.GetComponent<DropItemPrefab>();
            damageText.Initialize(trs.gameObject.transform, ItemId, Nickname, dropIdx);
        }
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
