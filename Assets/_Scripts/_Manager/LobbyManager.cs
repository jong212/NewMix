using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public void Button_Start()
    {
        //TEMPHIDE// Debug.Log("[1 LobbyManager : 1lobby => 2Down ]");
        SceneManager.LoadScene("2Down");
        
    }
}
