using Fusion;
using UnityEngine;

public class EnemyNetwork : NetworkBehaviour
{
    // 체력 값이 네트워크 상에서 동기화되며 변경이 감지되면 HealthChanged 호출
    [Networked, OnChangedRender(nameof(HealthChanged))]
    public float NetworkedHealth { get; set; } = 100;

    // 체력이 변경되면 호출됨
    void HealthChanged()
    {
        Debug.Log($"Health changed to: {NetworkedHealth}");
        // 체력이 변경될 때 체력바나 UI 업데이트 등의 후속 작업 수행
        UpdateHealthBar();
    }

    // 체력바를 업데이트하는 함수 (예시)
    void UpdateHealthBar()
    {
        // 체력바 UI 업데이트 로직
        Debug.Log($"Updating health bar to: {NetworkedHealth}");
    }

    // RPC를 통해 State Authority 클라이언트에서 체력을 감소시키는 함수
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void DealDamageRpc(float damage)
    {
        // 이 코드는 State Authority 클라이언트에서만 실행됨
        if (Object.HasStateAuthority)
        {
            // 체력을 감소시킴
            NetworkedHealth -= damage;
            Debug.Log($"Monster damaged! Remaining Health: {NetworkedHealth}");

            // 체력이 0 이하가 되면 몬스터를 죽임
            if (NetworkedHealth <= 0)
            {
                Die();
            }
        }
    }

    // 몬스터가 죽었을 때 처리
    void Die()
    {
        Debug.Log("Monster died.");
        // 사망 처리 로직 (예: 몬스터 제거)
        Destroy(gameObject);
    }
}
