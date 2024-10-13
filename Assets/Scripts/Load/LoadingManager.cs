using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// Down 씬에서 파일 다운로드가 완료 되면 현재 스크립트의 스태틱으로 된 메서드에 접근하여 nextScene을 Login 씬으로 설정한다 그로 인해 로딩화면이 잠시 보이고 로그인 화면으로 넘어가는 연출이 가능.
public class LoadingManager : MonoBehaviour
{
    public static string nextScene;
    public Slider loadingBar;

    private void Start()
    {
        StartCoroutine(StartLoadingScene());
    }

    IEnumerator StartLoadingScene()
    {
        yield return null;

        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;
        float timer = 0f;

        while (!op.isDone)
        {
            yield return null;
            timer += Time.deltaTime;

            if(op.progress < .9f)
            {
                loadingBar.value = Mathf.Lerp(loadingBar.value, op.progress, timer);
                if(loadingBar.value >= op.progress)
                {
                    timer = 0f;
                }
            } else
            {
                loadingBar.value = Mathf.Lerp(loadingBar.value,1f, timer);
                if(loadingBar.value == 1f)
                {
                    yield return new WaitForSeconds(2f);
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }

    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("3Loading");
    }
}
