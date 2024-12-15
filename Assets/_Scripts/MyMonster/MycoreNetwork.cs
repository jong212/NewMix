using Fusion;
using UnityEngine;
// Protected 를 사용하는 경우에는 해당클래스랑 자식클래스에서만 수정이 가능해야만 할 때 쓰자. 외부참조로 무변별한 수정을 막기 위함

public class MycoreNetwork : NetworkBehaviour
{
    protected virtual void Awake()
    {
    }
}