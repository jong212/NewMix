using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idx : MonoBehaviour
{
    [SerializeField] int chrIdx;

    // 읽기 전용 속성
    public int Charidx
    {
        get { return chrIdx; } // chrIdx 값을 반환
        private set { chrIdx = value; } // private set으로 외부 수정 제한
    }
}
