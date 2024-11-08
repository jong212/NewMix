using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataSetManager : MonoBehaviour
{
    private void Awake()
    {
        
    }
    public (string, string) CharacterDefaultSettings()
    {
        int cType = BackendGameData.Instance.userData.ChrType;

        foreach(CharacterSrcChart character in BackendGameData.Instance.CharacterList) // 차트 데이터
        {
            if(cType == character.charId)
            {
                return (character.labName,character.prefName);
            }
        }
        return ("","");
    }

    // 플레이어가 착용중인 장비의 인덱스 값들을 playerItemInfo 매게 변수로 넘겨 받음
    // 

    public void SetCharacterItem(NetworkArray<int> playerItemInfo, List<Transform> objPartsList)
    {

        var chartItemInfo = BackendGameData.Instance.ItemChartList;
        int idx = 0;
        foreach (int pItemInfo in playerItemInfo)
        {
            if (idx >= objPartsList.Count)
            {
                Debug.LogWarning($"Index out of bounds: idx ({idx}) is greater than objPartsList.Count ({objPartsList.Count})");
                break; // 또는 continue; 를 사용하여 다음 루프로 이동
            }

            if (pItemInfo == 0)
            {
                objPartsList[idx].DestroyChildren();
                // 여기서 idx를 증가시키지 않습니다.
            }
            else
            {
                foreach (ItemChart cItemInfo in chartItemInfo)
                {
                    if (pItemInfo == cItemInfo.Itemid)
                    {
                        GameObject obj = AddressableManager.instance.GetPrefab(cItemInfo.Label, cItemInfo.Prefabname);
                        Instantiate(obj, objPartsList[idx]);
                        break; // 원하는 경우 내부 루프만 탈출
                    }
                }
                // 필요한 경우 추가 로직을 여기에 작성
            }

            idx++; // idx 증가를 루프 끝에서 한 번만 수행
        }
    }

}
