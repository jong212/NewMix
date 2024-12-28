using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItemPoolManager : MonoBehaviour
{
    private WorldCanvas _worldCanvas;


    private void Start()
    {
        _worldCanvas = GetComponentInParent<WorldCanvas>();
    }

    public void ShowDropItem(NetworkObject trs, float ItemId,string Nickname,int dropIdx)
    {
        Vector3 randomOffset = new Vector3(
            UnityEngine.Random.Range(-3.0f, 3.0f),  // X축 랜덤 오프셋
            0f,                                     // Y축 고정 (기존 위치와 동일)
            UnityEngine.Random.Range(-3.0f, 3.0f)   // Z축 랜덤 오프셋
        );
        GameObject obj = _worldCanvas.GetPoolObject(PoolObjectType.DropItem);
       
        obj.transform.position = trs.gameObject.transform.position + randomOffset;
        obj.SetActive(true);

        // 텍스트 초기화
        DropItemPrefab damageText = obj.GetComponent<DropItemPrefab>();
        damageText.Initialize(trs.gameObject.transform, ItemId, Nickname, dropIdx);
       
    }

    public void ReturnToPool(GameObject obj)
    {
        _worldCanvas.CoolObject(obj, PoolObjectType.DropItem);
    }
}
