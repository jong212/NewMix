using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUI : MonoBehaviour
{
    protected string _errorTitle = "에러 발생";


    // ======================================================
    // 공통 에러처리 & 에러처리용 UI 
    // ======================================================
    protected void ShowAlertUI(string callback)
    {
        Debug.LogWarning(callback);
        StaticManager.UI.AlertUI.OpenWarningUI(_errorTitle, callback);
    }

}
