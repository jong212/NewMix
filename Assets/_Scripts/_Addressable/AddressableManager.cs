using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableManager : MonoBehaviour
{
    public static AddressableManager instance;

    private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();

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

    // Prefab Load Request with Label
    public void LoadPrefabsWithLabel(string label, System.Action onLoaded)
    {
        Addressables.LoadAssetsAsync<GameObject>(label, null).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (var prefab in handle.Result)
                {
                    prefabCache[prefab.name] = prefab;
                }
                Debug.Log("All prefabs successfully loaded from Addressables.");
                onLoaded?.Invoke();
            }
            else
            {
                Debug.LogError("Failed to load prefabs from Addressables: " + handle.OperationException);
            }
        };
    }

    // Get Prefab by Name
    public GameObject GetPrefab(string prefabName)
    {
        if (prefabCache.TryGetValue(prefabName, out GameObject prefab))
        {
            return prefab;
        }
        Debug.LogError("Prefab not found in cache: " + prefabName);
        return null;
    }

    // Release a Prefab (optional for memory management)
    public void ReleasePrefab(string prefabName)
    {
        if (prefabCache.TryGetValue(prefabName, out GameObject prefab))
        {
            Addressables.Release(prefab);
            prefabCache.Remove(prefabName);
            Debug.Log("Prefab released: " + prefabName);
        }
        else
        {
            Debug.LogWarning("Attempted to release a prefab that is not in cache: " + prefabName);
        }
    }
}