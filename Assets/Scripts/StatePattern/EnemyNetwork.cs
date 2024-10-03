using Fusion;
using UnityEngine;

public class EnemyNetwork : NetworkBehaviour
{
    // ü�� ���� ��Ʈ��ũ �󿡼� ����ȭ�Ǹ� ������ �����Ǹ� HealthChanged ȣ��
    [Networked, OnChangedRender(nameof(HealthChanged))]
    public float NetworkedHealth { get; set; } = 100;

    // ü���� ����Ǹ� ȣ���
    void HealthChanged()
    {
        Debug.Log($"Health changed to: {NetworkedHealth}");
        // ü���� ����� �� ü�¹ٳ� UI ������Ʈ ���� �ļ� �۾� ����
        UpdateHealthBar();
    }

    // ü�¹ٸ� ������Ʈ�ϴ� �Լ� (����)
    void UpdateHealthBar()
    {
        // ü�¹� UI ������Ʈ ����
        Debug.Log($"Updating health bar to: {NetworkedHealth}");
    }

    // RPC�� ���� State Authority Ŭ���̾�Ʈ���� ü���� ���ҽ�Ű�� �Լ�
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void DealDamageRpc(float damage)
    {
        // �� �ڵ�� State Authority Ŭ���̾�Ʈ������ �����
        if (Object.HasStateAuthority)
        {
            // ü���� ���ҽ�Ŵ
            NetworkedHealth -= damage;
            Debug.Log($"Monster damaged! Remaining Health: {NetworkedHealth}");

            // ü���� 0 ���ϰ� �Ǹ� ���͸� ����
            if (NetworkedHealth <= 0)
            {
                Die();
            }
        }
    }

    // ���Ͱ� �׾��� �� ó��
    void Die()
    {
        Debug.Log("Monster died.");
        // ��� ó�� ���� (��: ���� ����)
        Destroy(gameObject);
    }
}
