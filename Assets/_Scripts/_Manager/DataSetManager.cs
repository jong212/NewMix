using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataSetManager : MonoBehaviour
{
    public (string, string) CharacterDefaultSettings()
    {
        int cType = BackendGameData.Instance.userData.ChrType;

        foreach(CharacterSrcChart character in BackendGameData.Instance.CharacterList)
        {
            if(cType == character.charId)
            {
                Debug.Log(character.charId);
                Debug.Log(character.labName);
                Debug.Log(character.prefName);
            }
        }
        
        return ("", "test");
    }

}
