// �ڳ� SDK namespace �߰�
using BackEnd;
using UnityEngine;

public class BackendManager : MonoBehaviour
{
    public void Init()
    {
        var bro = Backend.Initialize(); // �ڳ� �ʱ�ȭ

        // �ڳ� �ʱ�ȭ�� ���� ���䰪
        if (bro.IsSuccess())
        {
            Debug.Log("�ڳ� �ʱ�ȭ ���� : " + bro); // ������ ��� statusCode 204 Success

        }
        else
        {
            Debug.LogError("�ʱ�ȭ ���� : " + bro); // ������ ��� statusCode 400�� ���� �߻�
        }
        //string googlehash = Backend.Utils.GetGoogleHash();

        /*
                 /*
                     // ��ųʸ� ���� (�� ���Ͽ� ���� ���� ���� ����Ʈ ����)
            Dictionary<string, List<int>> fruitData = new Dictionary<string, List<int>>()
            {
                { "���", new List<int> { 1, 2, 3 } },
                { "�ٳ���", new List<int> { 1, 2, 3 } }
            };

            // Param ��ü ���� �� ��ųʸ� �߰�
            Param param = new Param();
            param.Add("fruit_data", fruitData);

            // �ڳ��� ������ ����
            var bro = Backend.GameData.Insert("USER_DATA", param);

            if (bro.IsSuccess())
            {
                Debug.Log("������ ���� ���� : " + bro);
            }
            else
            {
                Debug.LogError("������ ���� ���� : " + bro);
            }

         */
    }

    private void NameSet()
    {

    }
}