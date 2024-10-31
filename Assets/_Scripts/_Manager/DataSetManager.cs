using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataSetManager : MonoBehaviour
{
    public (int,string) CharacterDefaultSettings()
    {
        int cType = BackendGameData.Instance.userData.ChrType;

        /*CharacterSrcChart getData = BackendGameData.Instance.CharacterList.FirstOrDefault ()*/
        return (1, "test");
    }

}
