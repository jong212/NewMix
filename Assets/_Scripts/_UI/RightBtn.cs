using UnityEngine;
using UnityEngine.UI;

public class RightBtn : MonoBehaviour
{
    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnRightButtonClick);
        }
    }

    private void OnRightButtonClick()
    {
        CharacterSelection characterSelection = LoginSceneManager.Instance?.Selecter;
        if (characterSelection != null)
        {
            characterSelection.RightBtnClick();
        }
        else
        {
            Debug.LogError("캐릭터 선택 화면 : 스크립트 null error");
        }
    }
}