using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataSetManager : MonoBehaviour
{
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

    // playerItemInfo : 뒤끝 데이터, 플레이어가 착용중인 장비의 아이템 인덱스 리스트
    // chartItemInfo : 차트 데이터 
    // playerItemInfo 인덱스 순서 정보와 objPartsList 인덱스 순서 정보를 맞춰야함

    public void SetCharacterItem(NetworkArray<int> playerItemInfo, List<Transform> objPartsList, List<Transform> objPartsPair)
    {
        Debug.Log(playerItemInfo);
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
                if(idx == 3)
                {
                    if(objPartsPair[0] != null)
                    {
                    objPartsPair[0].DestroyChildren();
                    }
                }
                // 여기서 idx를 증가시키지 않습니다.
            }
            else
            {

                foreach (ItemChart cItemInfo in chartItemInfo)
                {
                    if (pItemInfo == cItemInfo.Itemid)
                    {
                        if(objPartsList[idx].childCount > 0) objPartsList[idx].DestroyChildren();
                        GameObject obj = AddressableManager.instance.GetPrefab(cItemInfo.Label, cItemInfo.Prefabname);
                        Instantiate(obj, objPartsList[idx]);

                        if(idx == 3) // pair 신발 쌍
                        { 
                            if (objPartsList[idx].childCount > 0) objPartsPair[0].DestroyChildren();
                            Instantiate(obj, objPartsPair[0]);
                        }
                        break; // 원하는 경우 내부 루프만 탈출
                    }
                }
                // 필요한 경우 추가 로직을 여기에 작성
            }

            idx++; // idx 증가를 루프 끝에서 한 번만 수행
        }
    }

}
