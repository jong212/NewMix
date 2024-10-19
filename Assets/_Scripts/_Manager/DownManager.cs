using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
public class DownManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject waitMessage;
    public GameObject downMessage;

    public Slider downSlider;
    public Text sizeInfoText;
    public Text downValText;

    [Header("Label")]
    public AssetLabelReference defaultlabel;
    public AssetLabelReference player;
    public AssetLabelReference init;

    private long patchSize;
    private Dictionary<string, long>  patchMap = new Dictionary<string, long>();
    void Start()
    {
        waitMessage.SetActive(true);
        downMessage.SetActive(false);
        // ��巹���� �ʱ�ȭ
        StartCoroutine(InitAddressable());
        
        // �ٿ� �޾ƾ� �ϴ� ������ �ִ��� ������ üũ
        StartCoroutine(CheckUpdateFiles());

    }
    IEnumerator InitAddressable()
    {

        var init = Addressables.InitializeAsync();
        yield return init;
    }

    IEnumerator CheckUpdateFiles()
    {
        var labels = new List<string>() { defaultlabel.labelString, player.labelString,init.labelString };
        patchSize = default;

        foreach (var label in labels)
        {
            /*
            - ���ÿ� ���ҽ��� ������: GetDownloadSizeAsync�� 0�� ��ȯ.
            - �������� ���ҽ��� ������: GetDownloadSizeAsync�� �ش� ���ҽ��� �ٿ�ε� ũ�⸦ ��ȯ.
            - ���ҽ��� ������Ʈ�� ���: ������ ���ҽ� ũ�⸦ ��ȯ�Ͽ� ������Ʈ�� ���ҽ��� �ٿ�ε��ϰ� ����.
            */
            var handle = Addressables.GetDownloadSizeAsync(label);

            yield return handle;
            patchSize += handle.Result;
        }
        if(patchSize > decimal.Zero)
        {
            // ������Ʈ üũ�� �˾� �ݱ�
            waitMessage.SetActive(false);

            // �ٿ� �޾ƾ� �� ���� �ִٴ� �˾� ����
            downMessage.SetActive(true);

            // �ٿ� �޾ƾ��� ũ�� UI ǥ��
            sizeInfoText.text = GetFileSize(patchSize);
        }
        else // �ٿ� ���� �� ������ �� ����
        {
                downValText.text = "100 %";
                downSlider.value = 1f;
            yield return new WaitForSeconds(2f);
            LoadingManager.LoadScene("4Login");
            //LoadingManager.LoadScene("Preloader"); 4Login���� �ϴ°� �´µ� 4Login�����ϰ͵� ��� �ٷ� ���ӽ��� �غ��� �������� �� �ּ����� ��ü
        }

    }
    private string GetFileSize(long byteCnt)
    {
        string size = "0 Bytes";
        if(byteCnt >= 1073741824.0)
        {
            size = string.Format("{0:##.##}", byteCnt / 1073741824.0 + " GB");
        } 
        else if (byteCnt >= 1048576.0)
        {
            size = string.Format("{0:##.##}", byteCnt / 1048576.0 + " MB");
        }
        else if (byteCnt >= 1024.0)
        {
            size = string.Format("{0:##.##}", byteCnt / 1024.0 + " KB");
        } else if(byteCnt > 0 && byteCnt < 1024.0)
        {
            size = byteCnt.ToString() + " Bytes";
        }
        return size;
    }

    public void Button_DownLoad()
    {
        StartCoroutine(PatchFiles());
    }
    IEnumerator PatchFiles()
    {
        /*
        A �׷쿡 �� ���� ������Ʈ(��: ������ 1, ������ 2)�� �ִٰ� ������.
        �� �߿��� ������ 1���� "default" ���� �����Ǿ� �ְ�, ������ 2�� ���� �������� ���� ���¶�� �սô�.
        �� ���, Addressables.GetDownloadSizeAsync("default")�� ȣ���ϸ� "default" ���� ������ ���ҽ��� �ٿ�ε��� ũ�⸦ Ȯ���ϰ� ��. ��, ������ 1�� �ٿ�ε� ����� �ǰ�, ������ 2�� ����.        
        */
        var labels = new List<string>() { defaultlabel.labelString, player.labelString, init.labelString };

        foreach (var label in labels)
        {
            var handle = Addressables.GetDownloadSizeAsync(label);

            yield return handle;
            if(handle.Result != decimal.Zero)
            {
                StartCoroutine(DownLoadLabel(label));
            }
        }
        yield return CheckDownLoad();
    }
    IEnumerator DownLoadLabel(string label)
    {
        patchMap.Add(label, 0);
        //false�� �ٿ�ε� �� ���ҽ��� �ڵ����� �ε����� �ʰڴٴ� ����
        var handle = Addressables.DownloadDependenciesAsync(label,false);
        while (!handle.IsDone)
        {
            patchMap[label] = handle.GetDownloadStatus().DownloadedBytes;
            yield return new WaitForEndOfFrame();

        }
        patchMap[label] = handle.GetDownloadStatus().TotalBytes;
        /*
        �ڵ� ����: Release()�� �ٿ�ε��� ���ҽ��� �����ϴ� ���� �ƴ϶�, �ٿ�ε� �������� ���� handle�� ���� ������ �����ϰ� �޸� ������ �����ϱ� �����Դϴ�.
        handle�� �ٿ�ε� �۾��� �����ϴ� �񵿱� �۾� �ڵ��̸�, �۾��� �Ϸ�� �Ŀ��� �� �̻� �ʿ� �����Ƿ� �̸� �����ϴ� ���Դϴ�.
        �ڵ��� �������� ������, �񵿱� �۾��� ���õ� �޸𸮰� ��� ���� ���� �� �־� �޸� ������ �߻��� �� �ֽ��ϴ�.
        */
        Addressables.Release(handle);

    }
    IEnumerator CheckDownLoad()
    {
        var total = 0f;
        downValText.text = "0 %";
        while (true)
        {
            total += patchMap.Sum(tmp => tmp.Value);

            downSlider.value = total / patchSize;
            downValText.text = (int)(downSlider.value * 100) + " %";

            if(total == patchSize)
            {
                LoadingManager.LoadScene("4Login");
                //LoadingManager.LoadScene("Preloader"); 4Login���� �ϴ°� �´µ� 4Login�����ϰ͵� ��� �ٷ� ���ӽ��� �غ��� �������� �� �ּ����� ��ü                break;
            }
            total = 0f;
            yield return new WaitForEndOfFrame();   
        }
    }
}
