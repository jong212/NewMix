using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUpEffect : MonoBehaviour
{
    private Character _character;
    private RectTransform _rect;
    private int _endTime = 2;
    // Start is called before the first frame update
    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }
    private void OnEnable()
    {
        if(_character == null) _character = StaticManager.Instance.UniquePlayer;
        StartCoroutine(LevelUp());
    }
    private IEnumerator LevelUp()
    {
        float elapsed = 0f;
        Vector3 worldPos = _character.transform.position; // 몬스터의 월드 좌표

        // 카메라 방향에 따른 위치 보정
        _rect.position = worldPos + new Vector3(0, 1.2f, 0); // 초기 위치 설정 (몬스터 머리 위)

        // 텍스트 이동 거리와 방향 설정
        Vector3 initialPos = _rect.position;
        float moveDistance = 2.5f; // 텍스트가 올라갈 거리

        while (elapsed < _endTime)
        {
            // 텍스트가 위로 올라가는 Y 위치 계산
            float moveY = Mathf.Lerp(0, moveDistance, elapsed / _endTime);
            _rect.position = initialPos + new Vector3(0, moveY, 0); // 위로 이동

            elapsed += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(.3f);
        gameObject.SetActive(false); // 비활성화 (오브젝트 풀링)
        StaticManager.Instance.WorldCanvas.CoolObject(gameObject, PoolObjectType.LevelUp);

        yield break;
    }
    }

    
