using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
//https://chatgpt.com/share/671545ed-6cd0-800b-ae52-d92b932c3177
public class AddressableManager : MonoBehaviour
{
    public static AddressableManager instance;
    private Dictionary<string, List<GameObject>> prefabCache = new Dictionary<string, List<GameObject>>();
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // 넣은 라벨값에 해당하는 어드레서블에셋들을 "로드" 및 "캐싱"
    // 딕셔너리에 키 = 라벨이름 , 값 = 프리팹 이름 으로 맵핑 해서 오브젝트 추가함
    public void LoadPrefabsWithLabel(string label, System.Action onLoaded)
    {
        Addressables.LoadAssetsAsync<GameObject>(label, null).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (!prefabCache.ContainsKey(label))
                {
                    prefabCache[label] = new List<GameObject>();
                }

                foreach (var prefab in handle.Result) // 이 handle 사용법을 잘 익허야 할듯 디버그 해보니 내가 메모리 로드 및 인스턴스 한 오브젝트를 배열로도 가져온 것을 확인함 swoard1...2...3 그래서 그 이후 아래에서 캐싱 하는듯
                {
                    prefabCache[label].Add(prefab);
                    Debug.Log(" [어드레서블 로드 후 캐싱 완료] :" + prefab.name);
                }
                onLoaded?.Invoke();
            }
            else
            {
                Debug.LogError($"Failed to load prefabs with label '{label}' from Addressables: {handle.OperationException}");
            }
        };
    }
    public IEnumerator LoadPrefabsWithLabels(string label)
    {
        Debug.Log("[몬스터 스폰 과정 순서 메모 3]");

        var handle = Addressables.LoadAssetsAsync<GameObject>(label, null);
        yield return handle;
        Debug.Log("[몬스터 스폰 과정 순서 메모 5]");

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            if (!prefabCache.ContainsKey(label))
            {
                prefabCache[label] = new List<GameObject>();
            }

            foreach (var prefab in handle.Result)
            {
                prefabCache[label].Add(prefab);
                Debug.Log(" [어드레서블 로드 후 캐싱 완료] :" + prefab.name);
            }
        }
        else
        {
            Debug.LogError($"Failed to load prefabs with label '{label}' from Addressables: {handle.OperationException}");
        }
    }
     
    // (라벨 값 매게 변수로 받고) 오브젝트들 리스트 형태로 반환 함
    public List<GameObject> GetPrefabsByLabel(string label)
    {
        if (prefabCache.TryGetValue(label, out List<GameObject> prefabs))
        {
            return prefabs;
        }
        Debug.LogError($"No prefabs found with label: {label}");
        return new List<GameObject>();
    }

    //(라벨값,프리펩이름 매게 변수로 받고) 캐싱 된 단일 오브젝트를 프리팹 이름으로 찾아서 반환 

    public GameObject GetPrefab(string label, string prefabName)
    {
        if (prefabCache.TryGetValue(label, out List<GameObject> prefabs))
        {
            foreach (var prefab in prefabs)
            {
                if (prefab.name == prefabName)
                {
                    return prefab;
                }
            }
            Debug.LogError($"Prefab '{prefabName}' not found under label '{label}'.");
            return null;
        }
        Debug.LogError($"No prefabs found with label: {label}");
        return null;
    }
    // Release a Prefab (optional for memory management)
    public void ReleasePrefabsByLabel(string label)
    {
        if (prefabCache.TryGetValue(label, out List<GameObject> prefabs))
        {
            foreach (var prefab in prefabs)
            {
                Addressables.Release(prefab);
            }
            prefabCache.Remove(label);
            Debug.Log($"All prefabs with label '{label}' released.");
        }
        else
        {
            Debug.LogWarning($"No prefabs found with label: {label} to release.");
        }
    }
    public void LoadSpritesWithLabel(string label, Action onLoaded)
    {
        Addressables.LoadAssetsAsync<Sprite>(label, null).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (var sprite in handle.Result)
                {
                    if (!spriteCache.ContainsKey(sprite.name))
                    {
                        spriteCache[sprite.name] = sprite;
                        Debug.Log($"[스프라이트 로드 및 캐싱 완료] : {sprite.name}");
                    }
                }
                onLoaded?.Invoke();
            }
            else
            {
                Debug.LogError($"Failed to load sprites with label '{label}': {handle.OperationException}");
            }
        };
    }
    public Sprite GetSprite(string spriteName)
    {
        if (spriteCache.TryGetValue(spriteName, out Sprite sprite))
        {
            return sprite;
        }
        Debug.LogError($"Sprite '{spriteName}' not found in cache.");
        return null;
    }
    public void ReleaseSprites()
    {
        foreach (var sprite in spriteCache.Values)
        {
            Addressables.Release(sprite);
        }
        spriteCache.Clear();
        Debug.Log("All cached sprites released.");
    }
/*
    SpriteManager.instance.LoadSpritesWithLabel("MyLabel", () =>
{
    Debug.Log("모든 스프라이트 로드 및 캐싱 완료");
});
Sprite mySprite = SpriteManager.instance.GetSprite("SpriteName");
if (mySprite != null)
{
    // 스프라이트를 사용할 수 있습니다.
}
*/
}