using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleTonMono<T> : MonoBehaviour where T : MonoBehaviour, new()
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = GameObject.Find(typeof(T).ToString());
                if (go == null)
                    go = new GameObject(typeof(T).ToString());
                if (go.TryGetComponent<T>(out T inst))
                    _instance = inst;
                else
                    _instance = go.AddComponent<T>();
                DontDestroyOnLoad(_instance);
                (_instance as SingleTonMono<T>)?.Init();
            }
            return _instance;
        }
    }
    protected virtual void Init()
    {

    }
}
