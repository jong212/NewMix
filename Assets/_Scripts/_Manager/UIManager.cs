using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private AlertUI _alertUI;
    public AlertUI AlertUI
    {
        get
        {
            return _alertUI;
        }
    }
    public void Init()
    {
        AlertUI.gameObject.SetActive(false);
    }
    private bool TryLoadUIObject(string prefabName, Transform parent, out GameObject gameObject)
    {
        gameObject = null;

        string path = $"{prefabName}";
        GameObject loadObject = Resources.Load<GameObject>(path);

        if (loadObject == null)
        {
            Debug.LogError($"{prefabName}가 Prefab에 존재하지 않습니다. in {path}");
            return false;
        }

        gameObject = Object.Instantiate(loadObject, parent, true);
        gameObject.transform.localScale = Vector3.one;
        gameObject.transform.localPosition = Vector3.zero;

        return true;
    }
    public void OpenUI<T>(string folderPath, Transform parent)
    {
        if (TryLoadUIObject(folderPath + "/" + typeof(T).Name, parent, out var uiObject) == false)
        {
            Debug.LogError("Prefab No Error");
            return;
        }        
    }
}
