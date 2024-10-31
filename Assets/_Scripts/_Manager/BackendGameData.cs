using System.Text;
using UnityEngine;
using BackEnd;
using LitJson;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;
using Unity.VisualScripting;

#region 설계도 모음

// 설계도 - 어드레서블 매핑용 (현재 플레이어가 착용중인 캐릭터 ID 등등)
public class CharacterSrcChart
{
    public int charId { get; private set; }
    public string labName { get; private set; }
    public string prefName { get; private set; }

    public CharacterSrcChart(JsonData json)
    {
        charId = int.Parse(json["charId"].ToString());
        labName = json["labName"].ToString();
        prefName = json["prefName"].ToString();
    }
}

public class Test
{
    public int CharacterId { get; private set; }
    public string CharacterColor { get; private set; }
    public string Src { get; private set; }

    public Test(JsonData json)
    {
        CharacterId = int.Parse(json["CharacterId"].ToString());
        CharacterColor = json["CharacterColor"].ToString();
        Src = json["Src"].ToString();
    }
}

// 설계도 - 캐릭터 생성 시 플레이어 정보 DB세팅용 
public class UserData
{
    public int level = 1;
    public int money = 1;
    public int ChrType = 0;
    public int atk = 1;
    public string lastMap = "A";
    public int hp = 1;
    public int miss = 1;
    public override string ToString()  // 디버깅 위한 함수 (Debug.Log(UserData);)
    {
        StringBuilder result = new StringBuilder();

        result.AppendLine($"level : {level}");
        result.AppendLine($"money : {money}");
        result.AppendLine($"ChrType : {ChrType}");
        result.AppendLine($"atk : {atk}");
        result.AppendLine($"lastMap : {lastMap}");
        result.AppendLine($"hp : {hp}");
        result.AppendLine($"miss : {miss}");

        return result.ToString();
    }
}
// 설계도 - 서버, 로컬 차트 비교용 
public class ChartInfo
{
    public string chartName;
    public string chartFileId;
    public string updateDate;

    public ChartInfo(JsonData json)
    {
        chartName = json["chartName"].ToString();
        chartFileId = json["chartFileId"].ToString();
        updateDate = json["updateDate"].ToString();
    }
}
#endregion

public class BackendGameData 
{
 
    
    private static BackendGameData _instance = null;

    public static BackendGameData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new BackendGameData();
            }

            return _instance;
        }
    }
    // -----------------캐싱 Start-------------------------
    // 캐싱 종류 닉네임, 로컬 차트들, 서버에서 받아온 유저데이터
    private string _nickname { get; set; }   
    private List<string> _getCharLocalListname = new List<string>();
    private List<CharacterSrcChart> _characterList = new List<CharacterSrcChart>();
    private List<Test> _test = new List<Test>();
    public UserData userData;

    public string NickName                      // 캐싱 - NickName
    {
        get => _nickname;
        set => _nickname = value;
    }
    public List<string> GetCharLocalListname    // 캐싱 - Useable Local Charts
    {
        get => _getCharLocalListname;
        set => _getCharLocalListname = value;
    }
    public List<CharacterSrcChart> CharacterList // 캐싱 -기본 캐릭터 어드레서블 차트
    {
        get => _characterList;
        set => _characterList = value;
    }
    public List<Test> Test                       // 캐싱 -테스트
    {
        get => _test;
        set => _test = value;
    }
    public void SetNickname(string nickname)    // 캐싱 - 버튼 클릭 시 닉넴 캐싱 하는 건데 리펙토링 가능한지 체크해 봐야 할 듯 (중복코드라서)중복 버튼에서 바로 위 코드로 타는거가능한지 체크필요
    {
        NickName = nickname;
        Debug.Log($"[3-3 BackendGameData : 플레이어 닉네임 얼리 캐싱 {NickName}]");
    }
    // -----------------캐싱 End-------------------------

    public void InitSetting()
    {
        BackendGameData.Instance.GetPlayerData(); // 서버에서 데이터 새로 받아오기 위해 중복 초기화?

        foreach (var chartName in GetCharLocalListname)
        {
            LoadChart(chartName);
        }
        Matchmaker.Instance.TryConnectShared();

    }
    // 로컬에 최신화 된 차트 캐싱 작업 하는 메소드
    private void LoadChart(string chartName)
    {
        string chartDataString = Backend.Chart.GetLocalChartData(chartName);
        JsonData chartJson = JsonMapper.ToObject(chartDataString);
        chartJson = BackendReturnObject.Flatten(chartJson);

        switch (chartName)
        {
            case nameof(CharacterSrcChart):
                foreach (JsonData row in chartJson["rows"])
                {
                    CharacterSrcChart classRef = new CharacterSrcChart(row);
                    CharacterList.Add(classRef);
                }
                break;
            case nameof(Test):
                foreach (JsonData row in chartJson["rows"])
                {
                    Test classRef = new Test(row);
                    Test.Add(classRef);
                }
                break;
        }
    }

    private string gameDataRowInDate = string.Empty;

    // 닉네임을 설정하면서 유저 기본 정보 세팅 후 서버에 저장
    public void GameDataInsert(int? chrIdx) 
    {
        if (userData == null)
        {
            userData = new UserData();
        }

        Debug.Log("데이터를 초기화합니다.");
        userData.level = 1;
        userData.money = 10000;
        userData.ChrType = 1;
        userData.atk = 1;
        userData.lastMap = "A";
        userData.hp = 10;
        userData.miss = 1;

        Debug.Log("뒤끝 업데이트 목록에 해당 데이터들을 추가합니다.");
        Param param = new Param();
        param.Add("level", userData.level);
        param.Add("money", userData.money);
        param.Add("ChrType", chrIdx ?? userData.ChrType);
        param.Add("atk", userData.atk);
        param.Add("lastMap", userData.lastMap);
        param.Add("hp", userData.hp);
        param.Add("miss", userData.miss);


        Debug.Log("게임 정보 데이터 삽입을 요청합니다.");
        var bro = Backend.GameData.Insert("Character", param);

        if (bro.IsSuccess())
        {
            Debug.Log("게임 정보 데이터 삽입에 성공했습니다. : " + bro);

            //삽입한 게임 정보의 고유값입니다.  
            gameDataRowInDate = bro.GetInDate();            
        }
        else
        {
            Debug.LogError("게임 정보 데이터 삽입에 실패했습니다. : " + bro);
        }
    }

    // 플레이어 데이터 서버에서 가져와서 userData 변수에 캐싱하는 로직을 작성한 함수
    public void GetPlayerData()
    {
        var bro = Backend.GameData.GetMyData("Character", new Where());
        if (bro.IsSuccess())
        {
            Debug.Log($"[3-5 BackendGameData : 뒤끝에서 플레이어 정보 가져옴 {bro}]" );


            LitJson.JsonData gameDataJson = bro.FlattenRows(); // Json으로 리턴된 데이터를 받아옵니다.  

            
            if (gameDataJson.Count <= 0) // 받아온 데이터의 갯수가 0이라면 데이터가 존재하지 않는 것입니다.  
            {
                Debug.LogWarning("데이터가 존재하지 않습니다.");
            }
            else
            {
                gameDataRowInDate = gameDataJson[0]["inDate"].ToString(); //불러온 게임 정보의 고유값입니다.  

                userData = new UserData();

                userData.level = int.Parse(gameDataJson[0]["level"].ToString());
                userData.money = int.Parse(gameDataJson[0]["money"].ToString());
                userData.ChrType = int.Parse(gameDataJson[0]["ChrType"].ToString());
                userData.atk = int.Parse(gameDataJson[0]["atk"].ToString());
                userData.lastMap = gameDataJson[0]["lastMap"].ToString();
                userData.hp = int.Parse(gameDataJson[0]["hp"].ToString());
                userData.miss = int.Parse(gameDataJson[0]["miss"].ToString());

                Debug.Log($"[3-6 BackendGameData : 가져온 데이터 로컬에 캐싱 userData 여기 넣음 {userData.ToString()}]");
            }
        }
        else
        {
            Debug.LogError("게임 정보 조회에 실패했습니다. : " + bro);
        }
    }

    public void LevelUp()
    {
        Debug.Log("레벨을 1 증가시킵니다.");
        userData.level += 1;
        userData.atk += 1;
    }

    // 게임 정보 수정하기
    public void GameDataUpdate()
    {
        if (userData == null)
        {
            Debug.LogError("서버에서 다운받거나 새로 삽입한 데이터가 존재하지 않습니다. Insert 혹은 Get을 통해 데이터를 생성해주세요.");
            return;
        }

        Param param = new Param();
        param.Add("level", userData.level);
        param.Add("atk", userData.atk);

        BackendReturnObject bro = null;

        if (string.IsNullOrEmpty(gameDataRowInDate))
        {
            Debug.Log("내 제일 최신 게임 정보 데이터 수정을 요청합니다.");

            bro = Backend.GameData.Update("USER_DATA", new Where(), param);
        }
        else
        {
            Debug.Log($"{gameDataRowInDate}의 게임 정보 데이터 수정을 요청합니다.");

            bro = Backend.GameData.UpdateV2("USER_DATA", gameDataRowInDate, Backend.UserInDate, param);
        }

        if (bro.IsSuccess())
        {
            Debug.Log("게임 정보 데이터 수정에 성공했습니다. : " + bro);
        }
        else
        {
            Debug.LogError("게임 정보 데이터 수정에 실패했습니다. : " + bro);
        }
    }
    /*public void SetNickname(string nickname)
    {
        Nickname = nickname;
        Debug.Log($"닉네임 캐싱: {Nickname}");
    }*/
    
}