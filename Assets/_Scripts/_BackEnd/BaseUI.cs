using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUI : MonoBehaviour
{
    protected string _errorTitle = "���� �߻�";


    // ======================================================
    // ���� ����ó�� & ����ó���� UI 
    // ======================================================
    protected void ShowAlertUI(string callback)
    {
        Debug.LogWarning(callback);
        StaticManager.UI.AlertUI.OpenWarningUI(_errorTitle, callback);
    }

}
