using System.Collections.Generic;
using System.Text;
using UnityEngine;

// �ڳ� SDK namespace �߰�
using BackEnd;

public class UserData
{
    public int level = 1;
    public int money = 1;
    public int ChrType = 0;
    public int atk = 1;
    public int hp = 1;
    public int miss = 1;


    // �����͸� ������ϱ� ���� �Լ��Դϴ�.(Debug.Log(UserData);)
    public override string ToString()
    {
        StringBuilder result = new StringBuilder();

        result.AppendLine($"level : {level}");
        result.AppendLine($"money : {money}");
        result.AppendLine($"ChrType : {ChrType}");
        result.AppendLine($"atk : {atk}");
        result.AppendLine($"hp : {hp}");
        result.AppendLine($"miss : {miss}");

        return result.ToString();
    }
}

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

    public static UserData userData;
    public string Nickname { get; private set; }  // �г����� �����ϱ� ���� ������Ƽ

    private string gameDataRowInDate = string.Empty;
    public void GameDataInsert(int? chrIdx)
    {
        if (userData == null)
        {
            userData = new UserData();
        }

        Debug.Log("�����͸� �ʱ�ȭ�մϴ�.");
        userData.level = 1;
        userData.money = 10000;
        userData.ChrType = 1;
        userData.atk = 1;
        userData.hp = 10;
        userData.miss = 1;

        Debug.Log("�ڳ� ������Ʈ ��Ͽ� �ش� �����͵��� �߰��մϴ�.");
        Param param = new Param();
        param.Add("level", userData.level);
        param.Add("money", userData.money);
        param.Add("ChrType", chrIdx ?? userData.ChrType);
        param.Add("atk", userData.atk);
        param.Add("hp", userData.hp);
        param.Add("miss", userData.miss);


        Debug.Log("���� ���� ������ ������ ��û�մϴ�.");
        var bro = Backend.GameData.Insert("Character", param);

        if (bro.IsSuccess())
        {
            Debug.Log("���� ���� ������ ���Կ� �����߽��ϴ�. : " + bro);

            //������ ���� ������ �������Դϴ�.  
            gameDataRowInDate = bro.GetInDate();
        }
        else
        {
            Debug.LogError("���� ���� ������ ���Կ� �����߽��ϴ�. : " + bro);
        }
    }

    public void GameDataGet()
    {
        Debug.Log("���� ���� ��ȸ �Լ��� ȣ���մϴ�.");
        var bro = Backend.GameData.GetMyData("Character", new Where());
        if (bro.IsSuccess())
        {
            Debug.Log("���� ���� ��ȸ�� �����߽��ϴ�. : " + bro);


            LitJson.JsonData gameDataJson = bro.FlattenRows(); // Json���� ���ϵ� �����͸� �޾ƿɴϴ�.  

            // �޾ƿ� �������� ������ 0�̶�� �����Ͱ� �������� �ʴ� ���Դϴ�.  
            if (gameDataJson.Count <= 0)
            {
                Debug.LogWarning("�����Ͱ� �������� �ʽ��ϴ�.");
            }
            else
            {
                gameDataRowInDate = gameDataJson[0]["inDate"].ToString(); //�ҷ��� ���� ������ �������Դϴ�.  

                userData = new UserData();

                userData.level = int.Parse(gameDataJson[0]["level"].ToString());
                userData.money = int.Parse(gameDataJson[0]["money"].ToString());
                userData.ChrType = int.Parse(gameDataJson[0]["ChrType"].ToString());
                userData.atk = int.Parse(gameDataJson[0]["atk"].ToString());
                userData.hp = int.Parse(gameDataJson[0]["hp"].ToString());
                userData.miss = int.Parse(gameDataJson[0]["miss"].ToString());

                Debug.Log(userData.ToString());
            }
        }
        else
        {
            Debug.LogError("���� ���� ��ȸ�� �����߽��ϴ�. : " + bro);
        }
    }

    public void LevelUp()
    {
        Debug.Log("������ 1 ������ŵ�ϴ�.");
        userData.level += 1;
        userData.atk += 1;
    }

    // ���� ���� �����ϱ�
    public void GameDataUpdate()
    {
        if (userData == null)
        {
            Debug.LogError("�������� �ٿ�ްų� ���� ������ �����Ͱ� �������� �ʽ��ϴ�. Insert Ȥ�� Get�� ���� �����͸� �������ּ���.");
            return;
        }

        Param param = new Param();
        param.Add("level", userData.level);
        param.Add("atk", userData.atk);

        BackendReturnObject bro = null;

        if (string.IsNullOrEmpty(gameDataRowInDate))
        {
            Debug.Log("�� ���� �ֽ� ���� ���� ������ ������ ��û�մϴ�.");

            bro = Backend.GameData.Update("USER_DATA", new Where(), param);
        }
        else
        {
            Debug.Log($"{gameDataRowInDate}�� ���� ���� ������ ������ ��û�մϴ�.");

            bro = Backend.GameData.UpdateV2("USER_DATA", gameDataRowInDate, Backend.UserInDate, param);
        }

        if (bro.IsSuccess())
        {
            Debug.Log("���� ���� ������ ������ �����߽��ϴ�. : " + bro);
        }
        else
        {
            Debug.LogError("���� ���� ������ ������ �����߽��ϴ�. : " + bro);
        }
    }
    public void SetNickname(string nickname)
    {
        Nickname = nickname;
        Debug.Log($"�г��� ĳ��: {Nickname}");
    }
}