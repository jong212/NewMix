using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropItemPrefab : MonoBehaviour
{
    public GameObject SetDropItem;
    public Text ItemText;
    public PlayerRef PlayerRef;

    private Text textMesh;
    private RectTransform rectTransform;
    private float lifetime = 1f; // 텍스트가 사라지는 시간
    List<ItemChart> itemList;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        textMesh = GetComponent<Text>();
        itemList = BackendGameData.Instance.ItemChartList;
    }

    public void Initialize(Transform objTransform,float monsterId,string Nickname,int dropIdx)
    {
        if(SetDropItem.transform.childCount > 0)
        {
            foreach (Transform child in SetDropItem.transform)
            {
                ItemText.text = "";
                Destroy(child.gameObject);
            }

        }
        foreach (ItemChart item in itemList)
        {
            if (item.Itemid == dropIdx)
            {
                // Fetch the prefab using AddressableManager
                GameObject prefab = AddressableManager.instance.GetPrefab("Inventory", item.Prefabname);
                ItemText.text = item.ItemName;
                // Instantiate the prefab to avoid modifying the original asset
                Vector3 originalScale = prefab.transform.localScale;
                GameObject instantiatedObject = Instantiate(prefab);
                // Set the parent of the instantiated object
                instantiatedObject.transform.SetParent(SetDropItem.transform);
                instantiatedObject.transform.localScale = originalScale;// 이거 originalscale값 위에서 한 번 temp 한 이유는 instantiate할때 기존 scale값이 부모에의해 영향받아 값이 바끼는 문제가 있기 때문이고 인스턴스 이후에 원래 값으로 적용해서 문제 해결했음

                // Optionally reset the local position, rotation, and scale
                instantiatedObject.transform.localPosition = Vector3.zero;
                instantiatedObject.transform.localRotation = Quaternion.identity;
                //instantiatedObject.transform.localScale = Vector3.one;

                break;
            }
        }
        //StartCoroutine(FadeAndMove(objTransform));
    }

    private IEnumerator FadeAndMove(Transform trs)
    {
        float elapsed = 0f;
        Vector3 worldPos = trs.position; // 몬스터의 월드 좌표

        // 카메라 방향에 따른 위치 보정
        rectTransform.position = worldPos + new Vector3(0, 1.2f, 0); // 초기 위치 설정 (몬스터 머리 위)

        // 텍스트 이동 거리와 방향 설정
        Vector3 initialPos = rectTransform.position;
        float moveDistance = 2.5f; // 텍스트가 올라갈 거리

        while (elapsed < lifetime)
        {
            // 텍스트가 위로 올라가는 Y 위치 계산
            float moveY = Mathf.Lerp(0, moveDistance, elapsed / lifetime);
            rectTransform.position = initialPos + new Vector3(0, moveY, 0); // 위로 이동

            elapsed += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(.3f);
        gameObject.SetActive(false); // 비활성화 (오브젝트 풀링)
        StaticManager.UI.DamagePoolUI.ReturnToPool(gameObject);
    }

}
