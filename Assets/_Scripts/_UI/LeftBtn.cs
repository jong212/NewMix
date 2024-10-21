using UnityEngine;
using UnityEngine.UI;

public class LeftBtn : MonoBehaviour
{
    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnLeftButtonClick);
        }
    }

    private void OnLeftButtonClick()
    {
        CharacterSelection characterSelection = LoginSceneManager.Instance?.Selecter;
        if (characterSelection != null)
        {
            characterSelection.LeftBtnClick();
        }
        else
        {
            Debug.LogError("CharacterSelection ��ũ��Ʈ�� ã�� �� �����ϴ�.");
        }
    }
}