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

public class ItemChart
{
    public int Itemid { get; private set; }
    public string ItemName { get; private set; }
    public int Damage { get; private set; }
    public int MoveSpeed { get; private set; }
    public int SetLevel { get; private set; }
    public string Description { get; private set; }
    public string Label { get; private set; }
    public string Prefabname { get; private set; }

    public ItemChart(JsonData json)
    {
        Itemid = int.Parse(json["ItemId"].ToString());
        ItemName = json["ItemName"].ToString();
        Damage = int.Parse(json["Damage"].ToString());
        MoveSpeed = int.Parse(json["MoveSpeed"].ToString());
        SetLevel = int.Parse(json["SetLevel"].ToString());
        Description = json["Description"].ToString();
        Label = json["Label"].ToString();
        Prefabname = json["PrefabName"].ToString();
    }
}
public class MonsterInfoChart
{
    public class DropItems
    {

        public int Id { get; private set; }
        public int Percent { get; private set; }
        public DropItems(int id, int percent)
        {
            Id = id;
            Percent = percent;
        }
    }

    public int MonsterId { get; private set; }
    public string MonsterName { get; private set; }
    public string SceneName { get; private set; }
    public int Lv { get; private set; }
    public int Exp { get; private set; }
    public int Money { get; private set; }
    public int MonsterDropPercent { get; private set; }
    public List<DropItems> Dropitem { get; private set; }
    public string LabelName { get; private set; }
    public string PrafabName { get; private set; }
    public int Atk { get; private set; }
    public int Miss { get; private set; }
    public int Lucky { get; private set; }
    public int AgroDistance { get; private set; }
    public int AtkDistance { get; private set; }
    public int AtkCooldown { get; private set; }
    public float MoveSpeed { get; private set; }
    public int IdleTime { get; private set; }
    public int MoveTime { get; private set; }
    public int BattleTime { get; private set; }

    public MonsterInfoChart(JsonData json)
    {
        MonsterId = int.Parse(json["MonsterId"].ToString());
        MonsterName = json["MonsterName"].ToString();
        SceneName = json["SceneName"].ToString();
        Lv = int.Parse(json["Lv"].ToString());
        Exp = int.Parse(json["Exp"].ToString());
        Money = int.Parse(json["Money"].ToString());
        MonsterDropPercent = int.Parse(json["MonsterDropPercent"].ToString());
        Dropitem = new List<DropItems>();

        string dropItemListString = json["DropItem"].ToString();
        if (string.IsNullOrEmpty(dropItemListString) || dropItemListString == "null")
        {
            return;
        }
        JsonData dropItemListJson = JsonMapper.ToObject(dropItemListString);

        foreach (JsonData item in dropItemListJson)
        {
            int id = int.Parse(item["id"].ToString());
            int percent = int.Parse(item["percent"].ToString());
            Dropitem.Add(new DropItems(id, percent));
        }
        LabelName = json["LabelName"].ToString();
        PrafabName = json["PrafabName"].ToString();
        Atk = int.Parse(json["Atk"].ToString());
        Miss = int.Parse(json["Miss"].ToString());
        Lucky = int.Parse(json["Lucky"].ToString());
        AgroDistance = int.Parse(json["AgroDistance"].ToString());
        AtkDistance = int.Parse(json["AtkDistance"].ToString());
        AtkCooldown = int.Parse(json["AtkCooldown"].ToString());
        MoveSpeed = float.Parse(json["MoveSpeed"].ToString());
        IdleTime = int.Parse(json["IdleTime"].ToString());
        MoveTime = int.Parse(json["MoveTime"].ToString());
        BattleTime = int.Parse(json["BattleTime"].ToString());
        
    }

}

public class Node
{
    public bool walkable;         // 해당 노드를 지나갈 수 있는지 여부
    public Vector3 worldPosition; // 노드의 월드 좌표
    public int gridX;             // 그리드 상의 X 인덱스
    public int gridY;             // 그리드 상의 Y 인덱스

    public int gCost;             // 시작 노드로부터의 비용
    public int hCost;             // 목표 노드까지의 예상 비용
    public int fCost { get { return gCost + hCost; } } // 총 비용

    public Node parent;           // 경로 추적을 위한 부모 노드

    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }
}

// 설계도 - 캐릭터 생성 시 플레이어 정보 DB세팅용 
public class UserData
{
    private bool _isInitializing = false; // 초기화 여부를 나타내는 플래그

    private int _level;
    public int Level
    {
        get { return _level; }
        set
        {
            if (_level != value)
            {
                _level = value;
                if (!_isInitializing)
                {
                    BackendGameData.Instance.GameDataUpdate<int>("Level", value);
                }
            }
        }
    }
    private int _money;
    public int Money
    {
        get { return _money; }
        set
        {
            if (_money != value)
            {
                _money = value;
                if (!_isInitializing)
                {
                    BackendGameData.Instance.GameDataUpdate<int>("Money", value);
                }
            }
        }
    }
    private int _chrType;
    public int ChrType
    {
        get { return _chrType; }
        set
        {
            if (_chrType != value)
            {
                _chrType = value;
                if (!_isInitializing)
                {
                    BackendGameData.Instance.GameDataUpdate<int>("ChrType", value);
                }
            }
        }
    }
    private int _atk;
    public int Atk
    {
        get { return _atk; }
        set
        {
            if (_atk != value)
            {
                _atk = value;
                if (!_isInitializing)
                {
                    BackendGameData.Instance.GameDataUpdate<int>("Atk", value);
                }
            }
        }
    }
    private string _lastMap;
    public string LastMap
    {
        get { return _lastMap; }
        set
        {
            if (_lastMap != value)
            {
                _lastMap = value;
                if (!_isInitializing)
                {
                    BackendGameData.Instance.GameDataUpdate<string>("LastMap", value);
                }
            }
        }
    }
    private int _miss;
    public int Miss
    {
        get { return _miss; }
        set
        {
            if (_miss != value)
            {
                _miss = value;
                if (!_isInitializing)
                {
                    BackendGameData.Instance.GameDataUpdate<int>("Miss", value);
                }
            }
        }
    }
    private int _acc;
    public int Acc
    {
        get { return _acc; }
        set
        {
            if (_acc != value)
            {
                _acc = value;
                if (!_isInitializing)
                {
                    BackendGameData.Instance.GameDataUpdate<int>("Acc", value);
                }
            }
        }
    }
    private int _lucky;
    public int Lucky
    {
        get { return _lucky; }
        set
        {
            if (_lucky != value)
            {
                _lucky = value;
                if (!_isInitializing)
                {
                    BackendGameData.Instance.GameDataUpdate<int>("Lucky", value);
                }
            }
        }
    }
    public List<int> setPlayerItems = new List<int>();

    public override string ToString()  // 디버깅 위한 함수 (Debug.Log(UserData);)
    {
        StringBuilder result = new StringBuilder();

        result.AppendLine($"LV : {_level}");
        result.AppendLine($"money : {_money}");
        result.AppendLine($"LastMap : {_lastMap}");
        result.AppendLine($"ChrType : {ChrType}");
        result.AppendLine($"공격력 : {_atk}");
        result.AppendLine($"민첩 : {_miss}");
        result.AppendLine($"정확도 : {_acc}");
        result.AppendLine($"행운 : {_lucky}");
        foreach (var _value in setPlayerItems)
        {
            result.AppendLine($"보유아이템 : {_value}");
        }

        return result.ToString();
    }
    public void BeginInit()
    {
        _isInitializing = true;
    }

    // 초기화 종료 메서드
    public void EndInit()
    {
        _isInitializing = false;
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
    private List<CharacterSrcChart> _characterChartList = new List<CharacterSrcChart>();
    private List<ItemChart> _itemChartList = new List<ItemChart>();
    private List<MonsterInfoChart> _monsterInfoList = new List<MonsterInfoChart>();
    public UserData userData { get; set; }

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
        get => _characterChartList;
        set => _characterChartList = value;
    }
    public List<ItemChart> ItemChartList                       // 캐싱 -테스트
    {
        get => _itemChartList;
        set => _itemChartList = value;
    }
    public List<MonsterInfoChart> MonsterInfoList                       // 캐싱 -테스트
    {
        get => _monsterInfoList;
        set => _monsterInfoList = value;
    }
    public void SetNickname(string nickname)    // 캐싱 - 버튼 클릭 시 닉넴 캐싱 하는 건데 리펙토링 가능한지 체크해 봐야 할 듯 (중복코드라서)중복 버튼에서 바로 위 코드로 타는거가능한지 체크필요
    {
        NickName = nickname;
        Debug.Log($"[3-2] 캐싱완료 플레이어 닉네임 {NickName}");
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
    // 로컬에 최신화 된 차트들 캐싱 작업 하는 메소드
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
            case nameof(ItemChart):
                foreach (JsonData row in chartJson["rows"])
                {
                    ItemChart classRef = new ItemChart(row);
                    ItemChartList.Add(classRef);
                }
                break;
            case nameof(MonsterInfoChart):
                foreach (JsonData row in chartJson["rows"])
                {
                    MonsterInfoChart classRef = new MonsterInfoChart(row);
                    MonsterInfoList.Add(classRef);
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
        userData.BeginInit();
        Debug.Log("데이터를 초기화합니다.");
        userData.Level = 1;
        userData.Money = 10000;
        userData.ChrType = 1;
        userData.LastMap = "A";
        userData.setPlayerItems = new List<int>() { 1, 4 };//1,4는 
        userData.Atk = 1;
        userData.Acc = 10;
        userData.Miss = 1;
        userData.Lucky = 1;

        Debug.Log("뒤끝 업데이트 목록에 해당 데이터들을 추가합니다.");
        Param param = new Param();
        param.Add("Level", userData.Level);
        param.Add("Money", userData.Money);
        param.Add("ChrType", chrIdx ?? userData.ChrType);
        param.Add("LastMap", userData.LastMap);
        param.Add("SetPlayerItems", userData.setPlayerItems);
        param.Add("Atk", userData.Atk);
        param.Add("Miss", userData.Miss);
        param.Add("Acc", userData.Acc);
        param.Add("Lucky", userData.Lucky);

        Debug.Log("게임 정보 데이터 삽입을 요청합니다.");
        var bro = Backend.GameData.Insert("Character", param);
        userData.EndInit();
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
            LitJson.JsonData gameDataJson = bro.FlattenRows(); // Json으로 리턴된 데이터를 받아옵니다.  

            if (gameDataJson.Count <= 0) // 받아온 데이터의 갯수가 0이라면 데이터가 존재하지 않는 것입니다.  
            {
                Debug.LogWarning("데이터가 존재하지 않습니다.");
                GameDataInsert(0);
                GetPlayerData();
            }
            else
            {
                gameDataRowInDate = gameDataJson[0]["inDate"].ToString(); //불러온 게임 정보의 고유값입니다.  

                userData = new UserData();
                userData.BeginInit();

                userData.Level = int.Parse(gameDataJson[0]["Level"].ToString());
                userData.Money = int.Parse(gameDataJson[0]["Money"].ToString());
                userData.ChrType = int.Parse(gameDataJson[0]["ChrType"].ToString());
                userData.LastMap = gameDataJson[0]["LastMap"].ToString();
                userData.Atk = int.Parse(gameDataJson[0]["Atk"].ToString());
                userData.Miss = int.Parse(gameDataJson[0]["Miss"].ToString());
                userData.Acc = int.Parse(gameDataJson[0]["Acc"].ToString());
                userData.Lucky = int.Parse(gameDataJson[0]["Lucky"].ToString());

                userData.setPlayerItems.Clear();
                foreach (JsonData item in gameDataJson[0]["SetPlayerItems"])
                {
                    userData.setPlayerItems.Add(int.Parse(item.ToString()));
                }
                Debug.Log($"[3-4] 캐싱완료 userData 여기 넣음 {userData.ToString()}]");

                userData.EndInit();
            }
        }
        else
        {
            Debug.LogError("게임 정보 조회에 실패했습니다. : " + bro);
        }
    }

    public void GameDataUpdate<T>(string columName, T Parameter)
    {
        if (userData == null)
        {
            Debug.LogError("서버에서 다운받거나 새로 삽입한 데이터가 존재하지 않습니다. Insert 혹은 Get을 통해 데이터를 생성해주세요.");
            return;
        }

        Param param = new Param();
        param.Add(columName, Parameter);

        var bro = Backend.GameData.UpdateV2("Character", gameDataRowInDate, Backend.UserInDate, param);


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