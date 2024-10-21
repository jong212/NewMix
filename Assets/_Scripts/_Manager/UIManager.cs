using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
public enum UIType
{
    BackEndName,
    CharaterUI

}
public class UIManager : MonoBehaviour
{
    [SerializeField] private AlertUI _alertUI;
    [SerializeField] private ConfirmUI _confirmUI;

    // _createdUIDic 딕셔너리에 있으면 하이어라키에 존재한단 뜻
    private Dictionary<UIType, GameObject> _createdUIDic = new Dictionary<UIType, GameObject>();
    // _openedUIDic 여기 담겨있으면 SetActive True인 것임
    private HashSet<UIType> _openedUIDic = new HashSet<UIType>();

    public AlertUI AlertUI
    {
        get
        {
            return _alertUI;
        }
    }
    public ConfirmUI ConfirmUI
    {
        get
        {
            return _confirmUI;
        }
    }
    public void Init()
    {
        AlertUI.gameObject.SetActive(false);
        ConfirmUI.gameObject.SetActive(false);
    }
 

    // 공통 : UI 열어볼래?
    public void CommonOpen(UIType uiType , Transform parent, bool worldPositionStays)
    {
        var gObj = GetCreatedUI(uiType,  parent, worldPositionStays);

        if (gObj != null)
        {
            OpenUI(uiType, gObj);
        }
    }
    public GameObject CommonOpen(UIType uiType, Transform parent, bool worldPositionStays, bool returnObj )
    {
        var gObj = GetCreatedUI(uiType, parent, worldPositionStays);

        if (gObj != null)
        {
            OpenUI(uiType, gObj);
        }
        return returnObj ? gObj : null;
    }
    private GameObject GetCreatedUI(UIType uiType, Transform parent, bool worldPositionStays)
    {
        if (_createdUIDic.ContainsKey(uiType) == false)
        {
            CreateUI(uiType, parent, worldPositionStays);
        }
        return _createdUIDic[uiType];
    }
    private void CreateUI(UIType uiType, Transform parent, bool worldPositionStays)
    {
        if (_createdUIDic.ContainsKey(uiType) == false)
        {
            string path = GetUIPath(uiType);

            GameObject loadedObj = (GameObject)Resources.Load(path);
            GameObject gObj = Instantiate(loadedObj, parent, worldPositionStays);

            gObj.transform.localPosition = loadedObj.transform.localPosition;
            gObj.transform.localRotation = loadedObj.transform.localRotation;

            if (gObj != null)
            {
                _createdUIDic.Add(uiType, gObj);
            }
        }
    }
    private string GetUIPath(UIType uiType)
    {
        string path = string.Empty; // "" == string.Empty
        switch (uiType)
        {
            case UIType.BackEndName:
                path = "Prefabs/LoginScene/UI/BackEndSetName";
                break;
            case UIType.CharaterUI:
                path = "Prefabs/LoginScene/UI/CharaterUI";
                break;
        }
        return path;
    }
    private void OpenUI(UIType uiType, GameObject uiObject)
    {
        if (_openedUIDic.Contains(uiType) == false)
        {
            //OpenUI를 바로 타는 케이스가 있어서 비활성화 되어있는 오브젝트를 활성화 시키고 싶어서
            uiObject.SetActive(true);
            _openedUIDic.Add(uiType);
        }
    }
    public void CloseUI(UIType uiType)
    {
        if (_openedUIDic.Contains(uiType))
        {
            var uiObject = _createdUIDic[uiType];
            uiObject.SetActive(false);
            _openedUIDic.Remove(uiType);
        }
    }
}
