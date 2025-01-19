using BackEnd.Functions;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItemPoolManager : MonoBehaviour
{
    private WorldCanvas _worldCanvas;
    private Character _uniquePlayer;

    List<ItemChart> itemList;
    public float delayTime = .6f;       // 무기가 이동하기 전에 기다리는 시간
    private void Awake()
    {
        itemList = BackendGameData.Instance.ItemChartList;

    }
    private void Start()
    {

        _worldCanvas = GetComponentInParent<WorldCanvas>();
    }

    public void ShowDropItem(NetworkObject trs, float ItemId,string Nickname,int dropIdx)
    {
        _uniquePlayer = StaticManager.Instance.UniquePlayer;

        Vector3 randomOffset = new Vector3(
            UnityEngine.Random.Range(-2.0f, 2.0f),  // X축 랜덤 오프셋
            0f,                                     // Y축 고정 (기존 위치와 동일)
            UnityEngine.Random.Range(-2.0f, 2.0f)   // Z축 랜덤 오프셋
        );
        // GameObject obj = _worldCanvas.GetPoolObject(PoolObjectType.DropItem);
        Vector3 dropPosition = trs.transform.position + randomOffset;

        // 텍스트 초기화
        Initialize(dropPosition, ItemId, Nickname, dropIdx);
       
    }

    public void ReturnToPool(GameObject obj)
    {
        _worldCanvas.CoolObject(obj, PoolObjectType.DropItem);
    }
    public void Initialize(Vector3 objTransform, float monsterId, string Nickname, int dropIdx)
    {
       
        foreach (ItemChart item in itemList)
        {
            if (item.Itemid == dropIdx)
            {
                bool bEmptyChk = StaticManager.Instance.GetDropItemAction(dropIdx);
                // Fetch the prefab using AddressableManager
                GameObject prefab = AddressableManager.instance.GetPrefab("Inventory", item.Prefabname);
                //ItemText.text = item.ItemName;
                // Instantiate the prefab to avoid modifying the original asset
                Vector3 originalScale = prefab.transform.localScale;

                NetworkObject instantiatedObject = Matchmaker.Instance.Runner.Spawn(prefab, objTransform, Quaternion.identity);
                instantiatedObject.transform.localScale = originalScale;// 이거 originalscale값 위에서 한 번 temp 한 이유는 instantiate할때 기존 scale값이 부모에의해 영향받아 값이 바끼는 문제가 있기 때문이고 인스턴스 이후에 원래 값으로 적용해서 문제 해결했음
                instantiatedObject.transform.localRotation = Quaternion.identity;
                if (!bEmptyChk) { StartCoroutine(MoveToPlayerAfterDelay(instantiatedObject));};
                break;
            }
        }
    }
    private IEnumerator MoveToPlayerAfterDelay(NetworkObject instantiatedObject)
    {
        yield return new WaitForSeconds(delayTime);

        // 시작 속도와 최대 속도를 설정합니다.
        float currentSpeed = 0f;
        float maxSpeed = 10f; // 최대 속도
        float acceleration = 8f; // 가속도 (속도가 증가하는 비율)
        float deceleration = 1f; // 감속도 (속도가 감소하는 비율)
        float distance = Vector3.Distance(instantiatedObject.gameObject.transform.position, _uniquePlayer.transform.position);
        bool nullbreak = default;
        // Y축을 제외한 거리 계산
        while (distance > 0.3f)
        {
            if (instantiatedObject == null)
            {
                nullbreak = true;
                break;
            }
            // 목표 지점 계산
            Vector3 targetPosition = _uniquePlayer.transform.position;
            targetPosition.y = instantiatedObject.gameObject.transform.position.y; // Y축 고정

            // 거리 계산
            distance = Vector3.Distance(instantiatedObject.gameObject.transform.position, targetPosition);

            // 가속도 적용: 목표 위치로 가는 동안 속도 증가
            if (distance > 1.0f)
            {
                currentSpeed += acceleration * Time.deltaTime; // 가속도에 따라 속도 증가
            }
            else
            {
                // 목표 위치에 가까워지면 감속
                currentSpeed -= deceleration * Time.deltaTime; // 감속
            }

            // 속도가 최대 속도를 넘지 않도록 제한
            currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);

            // 무기 이동
            instantiatedObject.gameObject.transform.position = Vector3.MoveTowards(instantiatedObject.gameObject.transform.position, targetPosition, currentSpeed * Time.deltaTime);

            // 다음 프레임까지 기다림
            yield return null;
        }
        if (nullbreak == true) yield break;
        // 플레이어와의 거리가 거의 0에 가까워지면 무기 습득 처리
        Matchmaker.Instance.Runner.Despawn(instantiatedObject);
    }


}
