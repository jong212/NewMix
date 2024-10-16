using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InterfaceManager : MonoBehaviour
{
    #region Singleton

    public static InterfaceManager instance;

    private void Awake()
    {
        if (instance)
        {
            Debug.LogWarning("Instance already exists!");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    #endregion

    [Header("------ Canvases ------")]
    public Canvas mainCanvas;
    public Canvas worldCanvas;
    [Space]
    [Header("------ Screens ------")]
    public UIScreen MainMenuScreen;
    public UIScreen playerSettingScreen;
    public UIScreen gameplayHUD;
    public UIScreen kitchenConnectScreen;
    [Space]
    [Header("------ HUD ------")]
    public Transform foodOrderItemHolder;
    public TMP_Text roomNameText;
    public TMP_Text chefCounterText;
    public Animator chefIconAnimator;
    [Space]
    [Header("------ Player Settings Screen ------")]
    public TMP_InputField nicknameInputfield;

    public void SetNickname(string nick)
    {
        LocalData.nickname = nick;
    }

    public void PrintPlayerCount(int currentCount, int maxCount)
    {
        chefCounterText.text = $"{currentCount}/{maxCount}";
    }

    public void ChefIconShake()
    {
        if (chefIconAnimator.gameObject.activeInHierarchy)
            chefIconAnimator.SetTrigger("Shake");
    }

    public void ConfirmleaveSessionHook()
    {
        StartCoroutine(LeaveAndJoinNewSession());
    }
    private IEnumerator LeaveAndJoinNewSession()
    {
        Matchmaker.Instance.Runner.Shutdown();

        // 잠시 대기하여 Runner가 완전히 정리될 시간을 준다
        yield return new WaitForSeconds(1);


        Matchmaker.Instance.SetRoomCode("A");
        Matchmaker.Instance.TryConnectShared();
    }

}