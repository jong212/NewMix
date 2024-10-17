using BackEnd.Quobject.SocketIoClientDotNet.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticManager : MonoBehaviour
{
    public static StaticManager Instance { get; private set; }

    public static UIManager UI { get; private set; }


    // ��� ������ ���Ǵ� ��ɵ��� ��Ƴ��� Ŭ����.
    // �����޴����� ���� �ڿ� �����ϴ��� Ȯ�� �� �����Ѵ�.
    void Awake()
    {
        Init();
    }

    void Init()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        UI = GetComponentInChildren<UIManager>();

        UI.Init(); 


    }
}
