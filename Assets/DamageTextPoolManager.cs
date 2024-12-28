using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTextPoolManager : MonoBehaviour
{
    private WorldCanvas _worldCanvas;
    private void Start()
    {
        _worldCanvas = GetComponentInParent<WorldCanvas>();
    }
    public void ShowDamage(NetworkObject trs, string damage)
    {

        GameObject obj = _worldCanvas.GetPoolObject(PoolObjectType.AttackViewText);

            obj.transform.position = trs.gameObject.transform.position;
            obj.SetActive(true);

            // 텍스트 초기화
            DamageText damageText = obj.GetComponent<DamageText>();
            damageText.Initialize(trs.gameObject.transform,damage);
        
    }

    public void ReturnToPool(GameObject obj)
    {
         _worldCanvas.CoolObject(obj,PoolObjectType.AttackViewText);

    }
}
