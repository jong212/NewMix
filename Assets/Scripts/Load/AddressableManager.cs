using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
public class AddressableManager : MonoBehaviour
{
    [SerializeField]
    private AssetReferenceGameObject charecterObj;

    private List<GameObject> gameObjects = new List<GameObject>();
    void Start()
    {
        Button_SpawnObject();
    }
    IEnumerator InitAddressable()
    {
        var init = Addressables.InitializeAsync();
        yield return init;
    }

    public void Button_SpawnObject()
    {
        charecterObj.InstantiateAsync().Completed += (obj) =>
        {
            gameObjects.Add(obj.Result);
        };
    }
    void Update()
    {
        
    }
}
