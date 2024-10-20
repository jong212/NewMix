using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelection : MonoBehaviour
{
    public List<GameObject> characters = new List<GameObject>(); // ���� �ν��Ͻ�ȭ�� ĳ���͵��� ������ ����Ʈ
    public int selectedCharacter = 0;

    // ��巹������ SelectPlayer�� ���� �Ǿ��ִ� �͵� ��� �޸𸮿� �ø� �� ĳ�̿����� ��ųʸ��� ��Ƶд� ���⼭ ���� Ű�� ��Ƶα� ������ ���� Ű�� �����ؼ� GetPrefabByLabel �Լ��� ����ϸ� �� ���� ����Ʈ�� ������ �� �ִ�
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
                characters.Add(instantiatedCharacter); // �ν��Ͻ�ȭ�� ������Ʈ�� ����Ʈ�� �߰�
                StaticManager.Instance.LogHierarchyPath(instantiatedCharacter.transform);
            }
        });
    }

    // ���� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    public void LeftBtnClick()
    {
        // ���� Ȱ��ȭ�� ĳ���͸� ��Ȱ��ȭ
        characters[selectedCharacter].SetActive(false);

        // �ε����� �������� �̵� (���� 0��°��� ����Ʈ�� ������ ���ư���)
        selectedCharacter--;
        if (selectedCharacter < 0)
        {
            selectedCharacter = characters.Count - 1; // ����Ʈ�� ������ �ε����� �̵�
        }

        // ���ο� ĳ���� Ȱ��ȭ
        characters[selectedCharacter].SetActive(true);
    }

    // ������ ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    public void RightBtnClick()
    {
        // ���� Ȱ��ȭ�� ĳ���͸� ��Ȱ��ȭ
        characters[selectedCharacter].SetActive(false);

        // �ε����� ���������� �̵� (���� �������̶�� ù ��°�� ���ư���)
        selectedCharacter++;
        if (selectedCharacter >= characters.Count)
        {
            selectedCharacter = 0; // ����Ʈ�� ù ��° �ε����� �̵�
        }

        // ���ο� ĳ���� Ȱ��ȭ
        characters[selectedCharacter].SetActive(true);
    }
}
