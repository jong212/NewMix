using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingChangeTextUpdate : MonoBehaviour
{
    [SerializeField] private Text _changeText;
    private string _baseText = "Loading"; // 기본 텍스트
    private int _dotCount = 0;

    void OnEnable()
    {
        // 활성화될 때 코루틴 시작
        StartCoroutine(UpdateLoadingText());
    }
    void OnDisable()
    {
        // 비활성화될 때 코루틴 중지
        StopAllCoroutines();
    }
    private IEnumerator UpdateLoadingText()
    {
        while (true)
        {
            // 점 개수에 따라 텍스트 업데이트
            _changeText.text = _baseText + new string('.', _dotCount);

            // 점 개수 업데이트 (0~3 순환)
            _dotCount = (_dotCount + 1) % 4;

            // 0.5초 대기 후 다음 업데이트
            yield return new WaitForSeconds(0.5f);
        }
    }
}
