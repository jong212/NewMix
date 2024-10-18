using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConfirmUI;

public class BaseUI : MonoBehaviour
{
    // ======================================================
    // 공통 에러처리 & 에러처리용 UI 
    // ======================================================
    protected void ShowAlertUI(string titleText, string callback)
    {
        Debug.LogWarning(callback);
        StaticManager.UI.AlertUI.OpenWarningUI(titleText, callback);
    }
    protected void ShowConfirmUI(string titleText = null, string infoText = null, string okBtnText = null, string cancelBtnText = null, ClickConfirmOkButton okCallback = null)
    {
        StaticManager.UI.ConfirmUI.OpenConfirmUI(
        titleText ?? null,
        infoText ?? null,
        okBtnText ?? null,
        cancelBtnText ?? null,
        okCallback ?? null
        );
    }

}
