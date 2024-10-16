// 뒤끝 SDK namespace 추가
using BackEnd;
using UnityEngine;

public class BackendManager : MonoBehaviour
{
    void Start()
    {
        var bro = Backend.Initialize(); // 뒤끝 초기화

        // 뒤끝 초기화에 대한 응답값
        if (bro.IsSuccess())
        {
            Debug.Log("초기화 성공 : " + bro); // 성공일 경우 statusCode 204 Success
        }
        else
        {
            Debug.LogError("초기화 실패 : " + bro); // 실패일 경우 statusCode 400대 에러 발생
        }
        StartGoogleLogin();
        string googlehash = Backend.Utils.GetGoogleHash();
        Debug.Log(googlehash);

        /*
                     // 딕셔너리 생성 (각 과일에 여러 값을 가진 리스트 형태)
            Dictionary<string, List<int>> fruitData = new Dictionary<string, List<int>>()
            {
                { "사과", new List<int> { 1, 2, 3 } },
                { "바나나", new List<int> { 1, 2, 3 } }
            };

            // Param 객체 생성 후 딕셔너리 추가
            Param param = new Param();
            param.Add("fruit_data", fruitData);

            // 뒤끝에 데이터 삽입
            var bro = Backend.GameData.Insert("USER_DATA", param);

            if (bro.IsSuccess())
            {
                Debug.Log("데이터 삽입 성공 : " + bro);
            }
            else
            {
                Debug.LogError("데이터 삽입 실패 : " + bro);
            }

         */
    }
    public void StartGoogleLogin()
    {
        TheBackend.ToolKit.GoogleLogin.Android.GoogleLogin(true, GoogleLoginCallback);
    }

    private void GoogleLoginCallback(bool isSuccess, string errorMessage, string token)
    {
        if (isSuccess == false)
        {
            Debug.LogError(errorMessage);
            return;
        }

        Debug.Log("구글 토큰 : " + token);
        var bro = Backend.BMember.AuthorizeFederation(token, FederationType.Google);
        Debug.Log("페데레이션 로그인 결과 : " + bro);
    }
}