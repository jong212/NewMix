using BackEnd.Quobject.SocketIoClientDotNet.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

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

    // ����Ͽ��� ������Ʈ ��ġ Ȯ�ο�
    public void LogHierarchyPath(Transform transform)
    {
        if (transform == null)
        {
            Debug.LogError("Transform is null. Cannot log hierarchy path.");
            return;
        }

        string path = transform.name;
        Transform currentParent = transform.parent;

        // �θ� ���� �ö󰡸� ��ü ��� ����
        while (currentParent != null)
        {
            path = currentParent.name + "/" + path;
            currentParent = currentParent.parent;
        }

        // �̸��� ��θ� �α׷� ���
        Debug.Log($"Object name: {transform.name}, Path: {path}");
    }
}
