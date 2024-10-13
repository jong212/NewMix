using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// Down ������ ���� �ٿ�ε尡 �Ϸ� �Ǹ� ���� ��ũ��Ʈ�� ����ƽ���� �� �޼��忡 �����Ͽ� nextScene�� Login ������ �����Ѵ� �׷� ���� �ε�ȭ���� ��� ���̰� �α��� ȭ������ �Ѿ�� ������ ����.
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
