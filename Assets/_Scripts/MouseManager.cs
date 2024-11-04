// MouseManager.cs
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public LayerMask monsterLayerMask; // 몬스터가 있는 레이어 마스크
    public float sphereRadius = 0.5f;  // 레이의 굵기를 높힘으로서 몬스터 검출 확률을 높힘

    public delegate void OnMonsterClickedHandler(Transform monsterTransform);
    public event OnMonsterClickedHandler OnMonsterClicked;

    private Ray lastRay;              // 디버그용 
    private bool lastHit; 

    public void ClickCheck()
    {
        
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            Vector3 adjustedOrigin = ray.origin;                      // Debug.Log("카메라 위치 레이저 시작점: " + ray.origin);            
            Ray adjustedRay = new Ray(adjustedOrigin, ray.direction); //Debug.Log("카메라 위치 레이저 방향: " + ray.direction);
            RaycastHit hit;

            lastRay = adjustedRay;
            lastHit = false;
            
            if (Physics.SphereCast(adjustedRay, sphereRadius, out hit, 30f, monsterLayerMask)) // 
            {
                // 몬스터를 클릭한 경우
                Transform monsterTransform = hit.transform;
                Debug.Log("몬스터 클릭됨: " + monsterTransform.name);

                // 이벤트 발생
                OnMonsterClicked?.Invoke(monsterTransform);
                lastHit = true;
            }
            else
            {
                Debug.Log("클릭한 위치에 몬스터가 없습니다.");
            }
         
    }

    void OnDrawGizmos()
    {
        if (lastRay.direction != Vector3.zero)
        {
            Gizmos.color = lastHit ? Color.red : Color.yellow;
            Gizmos.DrawSphere(lastRay.origin + lastRay.direction * 30f, sphereRadius);
            Gizmos.DrawLine(lastRay.origin, lastRay.origin + lastRay.direction * 30f);
        }
    }
}
