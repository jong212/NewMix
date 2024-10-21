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
            Debug.LogError("CharacterSelection 스크립트를 찾을 수 없습니다.");
        }
    }
}