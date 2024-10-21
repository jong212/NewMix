using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWaitRoomInfo : MonoBehaviour
{
    [SerializeField] Text nickNameLayout;
    [SerializeField] Text lvLayout;
    [SerializeField] Text atkLayout;
    [SerializeField] Text pointLayout;
    [SerializeField] Text hpLayout;
    [SerializeField] Text missLayout;
    [SerializeField] Text lastLocationLayout;
    [SerializeField] Button deleteCharacter;
    [SerializeField] Button gameStartButton;
    public Text NickNameLayout { get => nickNameLayout; set => nickNameLayout = value; }
    public Text LvLayout { get => lvLayout; set => lvLayout = value; }
    public Text AtkLayout { get => atkLayout; set => atkLayout = value; }
    public Text PointLayout { get => pointLayout; set => pointLayout = value; }
    public Text HpLayout { get => hpLayout; set => hpLayout = value; }
    public Text MissLayout { get => missLayout; set => missLayout = value; }
    public Text LastLocationLayout { get => lastLocationLayout; set => lastLocationLayout = value; }
    public Button DeleteCharacter { get => deleteCharacter; set => deleteCharacter = value; }
    public Button GameStartButton { get => gameStartButton; set => gameStartButton = value; }

    private void Awake()
    {
        if (nickNameLayout == null || lvLayout == null || atkLayout == null || pointLayout == null || hpLayout == null || missLayout == null || lastLocationLayout == null || deleteCharacter == null || gameStartButton == null)
        {
            Debug.LogError("UI 요소가 연결되지 않았습니다!");
        }
    }
}
