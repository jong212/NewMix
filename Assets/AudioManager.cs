using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [Header("#LoginScene")]
    public AudioClip bgmClip;
    public float bgmVolume;
    public AudioSource bgmPlayer;    
    
    [Header("#BattleScene")]
    public AudioClip battleClip;
    public float battleVolume;
    public AudioSource battleSource;

    [Header("#SFX")]
    public AudioClip[] sfxClips;
    public float sfxVolume;
    public int channels;
    public AudioSource[] sfxPlayers;
    int channelIndex;

    public enum Sfx { Attack, Itemsloat,Click}
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Init()
    {
        // 배경음 초기화
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmClip;
        bgmPlayer.Play(); 
        bgmPlayer.loop = true;

        // 전투씬 초기화
        GameObject battleObject = new GameObject("BattleScene");
        battleObject.transform.parent = transform;
        battleSource = battleObject.AddComponent<AudioSource>();
        battleSource.playOnAwake = false;
        battleSource.volume = battleVolume;
        battleSource.clip = battleClip;
        battleSource.Play();
        battleSource.loop = true;

        // 효과음 초기화
        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels]; // 채널 개수 만큼 배열 초기화 

        for (int index =0; index < sfxPlayers.Length; index++)
        {
            sfxPlayers[index] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[index].playOnAwake = false;
            sfxPlayers[index].volume = sfxVolume;
        }
         
    }

    public void PlaySfx(Sfx sfx)
    {
        for (int index = 0; index < sfxPlayers.Length; index++)
        {
            int loopIndex = (index + channelIndex) % sfxPlayers.Length;
            sfxPlayers[(int)sfx].clip = sfxClips[(int)sfx];
            sfxPlayers[(int)sfx].Play();
            break;
        }
    }
}
