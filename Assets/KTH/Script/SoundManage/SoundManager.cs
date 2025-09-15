using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundEffect
{
    public string name;
    public AudioClip clip;

    [Tooltip("볼륨 (0.0 ~ 1.0)")]
    [Range(0f, 1f)]
    public float volume = 1.0f;
}


public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioSource bgmSource;
    public AudioSource sfxSource;

    // 인스펙터에서 BGM과 SFX를 따로 관리
    [Header("사운드 클립 목록")]
    public SoundEffect[] BGMClips;
    public SoundEffect[] SFXClips;

    private Dictionary<string, SoundEffect> bgmDict = new Dictionary<string, SoundEffect>();
    private Dictionary<string, SoundEffect> sfxDict = new Dictionary<string, SoundEffect>();

    void Awake()
    {
        // 싱글턴
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // BGM 딕셔너리 초기화
        foreach (var bgm in BGMClips)
        {
            bgmDict.Add(bgm.name, bgm);
        }

        // SFX 딕셔너리 초기화
        foreach (var sfx in SFXClips)
        {
            sfxDict.Add(sfx.name, sfx);
        }
    }

    // 배경음 재생
    public void PlayBGM(string bgmName)
    {
        if (bgmDict.TryGetValue(bgmName, out SoundEffect sound))
        {
            bgmSource.clip = sound.clip;
            bgmSource.volume = sound.volume;
            bgmSource.loop = true;
            bgmSource.Play();
        }
        else Debug.LogWarning("BGM not found: " + bgmName);
    }
    
    // 배경음 중지
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // 효과음 재생 (단발성)
    public void PlaySFX(string sfxName)
    {
        if (sfxDict.TryGetValue(sfxName, out SoundEffect sound))
        {
            sfxSource.PlayOneShot(sound.clip, sound.volume);
        }
        else Debug.LogWarning("SFX not found: " + sfxName);
    }

    // 3D 효과음 재생 (단발성)
    public void PlaySFXAtPoint(string sfxName, Vector3 position)
    {
        if (sfxDict.TryGetValue(sfxName, out SoundEffect sound))
        {
            AudioSource.PlayClipAtPoint(sound.clip, position, sound.volume);
        }
        else Debug.LogWarning("SFX not found: " + sfxName);
    }
}