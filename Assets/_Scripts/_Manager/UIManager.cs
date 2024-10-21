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

    // _createdUIDic ��ųʸ��� ������ ���̾��Ű�� �����Ѵ� ��
    private Dictionary<UIType, GameObject> _createdUIDic = new Dictionary<UIType, GameObject>();
    // _openedUIDic ���� ��������� SetActive True�� ����
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
 

    // ���� : UI �����?
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
            //OpenUI�� �ٷ� Ÿ�� ���̽��� �־ ��Ȱ��ȭ �Ǿ��ִ� ������Ʈ�� Ȱ��ȭ ��Ű�� �;
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
