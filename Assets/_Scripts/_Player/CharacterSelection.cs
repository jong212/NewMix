using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelection : MonoBehaviour
{
    public List<GameObject> characters = new List<GameObject>(); // 실제 인스턴스화된 캐릭터들을 저장할 리스트
    public int selectedCharacter = 0;

    // 어드레서블에서 SelectPlayer로 설정 되어있는 것들 모두 메모리에 올린 후 캐싱용으로 딕셔너리에 담아둔다 여기서 라벨을 키로 담아두기 때문에 라벨을 키로 접근해서 GetPrefabByLabel 함수를 사용하면 한 번에 리스트로 가져올 수 있다
    private void Awake()
    {
        if (AddressableManager.instance == null) Debug.LogError("AddressableManager instance is not initialized.");

        AddressableManager.instance.LoadPrefabsWithLabel("SelectPlayer", () =>
        {
 
            var loadedPrefabs = AddressableManager.instance.GetPrefabsByLabel("SelectPlayer");

            bool actives = false;
            foreach (GameObject prefab in loadedPrefabs)
            {
                
                GameObject instantiatedCharacter = Instantiate(prefab, transform);
                
                if (!actives) {
                    instantiatedCharacter.SetActive(true);
                    actives = true;
                } else
                {
                    instantiatedCharacter.SetActive(false);
                }
                characters.Add(instantiatedCharacter); // 인스턴스화된 오브젝트를 리스트에 추가
                StaticManager.Instance.LogHierarchyPath(instantiatedCharacter.transform);
            }
        });
    }

    // 왼쪽 버튼 클릭 시 호출되는 함수
    public void LeftBtnClick()
    {
        // 현재 활성화된 캐릭터를 비활성화
        characters[selectedCharacter].SetActive(false);

        // 인덱스를 왼쪽으로 이동 (만약 0번째라면 리스트의 끝으로 돌아가기)
        selectedCharacter--;
        if (selectedCharacter < 0)
        {
            selectedCharacter = characters.Count - 1; // 리스트의 마지막 인덱스로 이동
        }

        // 새로운 캐릭터 활성화
        characters[selectedCharacter].SetActive(true);
    }

    // 오른쪽 버튼 클릭 시 호출되는 함수
    public void RightBtnClick()
    {
        // 현재 활성화된 캐릭터를 비활성화
        characters[selectedCharacter].SetActive(false);

        // 인덱스를 오른쪽으로 이동 (만약 마지막이라면 첫 번째로 돌아가기)
        selectedCharacter++;
        if (selectedCharacter >= characters.Count)
        {
            selectedCharacter = 0; // 리스트의 첫 번째 인덱스로 이동
        }

        // 새로운 캐릭터 활성화
        characters[selectedCharacter].SetActive(true);
    }
}
