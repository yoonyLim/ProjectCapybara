using System.Collections.Generic;
using UnityEngine;

// [System.Serializable] 어트리뷰트: Unity 인스펙터에서 이 클래스의 필드를 보고 수정할 수 있게 해줍니다.
/// <summary>
/// 개별 사운드 클립의 정보를 담는 클래스입니다.
/// </summary>
[System.Serializable]
public class SoundEffect
{
    public string name;     // 사운드의 이름 (파일 이름과 동일하게 사용됨)
    public AudioClip clip;   // 실제 오디오 데이터
    [Range(0f, 1f)] public float volume = 1.0f; // 사운드 클립의 개별 볼륨
}

/// <summary>
/// 게임의 모든 사운드(BGM, SFX)를 관리하는 싱글톤 클래스입니다.
/// </summary>
public class SoundManager : MonoBehaviour
{
    // 싱글톤(Singleton) 패턴: 게임 내에 SoundManager 인스턴스가 단 하나만 존재하도록 보장합니다.
    public static SoundManager instance;

    // 사운드를 재생할 AudioSource 컴포넌트들
    public AudioSource bgmSource;  // 배경음악(BGM)을 재생할 오디오 소스
    public AudioSource sfxSource;  // 효과음(SFX)을 재생할 오디오 소스

    // 사운드 클립 목록을 인스펙터에서 확인할 수 있도록 public으로 선언
    [Header("사운드 클립 목록 (자동으로 채워집니다)")]
    public SoundEffect[] BGMClips; // BGM 사운드 목록 배열
    public SoundEffect[] SFXClips; // SFX 사운드 목록 배열

    // 사운드를 이름(string)으로 빠르게 찾기 위한 딕셔너리(Dictionary)
    private Dictionary<string, SoundEffect> bgmDict = new Dictionary<string, SoundEffect>();
    private Dictionary<string, SoundEffect> sfxDict = new Dictionary<string, SoundEffect>();

    // 게임 오브젝트가 생성될 때 Awake 함수가 가장 먼저 호출됩니다.
    void Awake()
    {
        // --- 싱글톤 패턴 구현부 ---
        if (instance == null) // instance가 아직 할당되지 않았다면
        {
            instance = this; // 이 인스턴스를 static instance로 사용
            DontDestroyOnLoad(gameObject); // 씬(Scene)이 바뀌어도 이 게임 오브젝트가 파괴되지 않도록 설정
        }
        else // instance가 이미 존재한다면 (다른 씬에서 넘어온 경우 등)
        {
            Destroy(gameObject); // 새로 생성된 이 게임 오브젝트는 파괴
            return;
        }

        // --- 사운드 클립 자동 로드 및 초기화 ---

        // 1. Resources/BGM 폴더에서 모든 AudioClip 파일을 불러옵니다.
        AudioClip[] bgmResources = Resources.LoadAll<AudioClip>("BGM");
        BGMClips = new SoundEffect[bgmResources.Length]; // 불러온 파일 개수만큼 배열 크기 할당
        for (int i = 0; i < bgmResources.Length; i++)
        {
            // SoundEffect 객체를 생성하고 정보를 채워 배열에 넣습니다.
            BGMClips[i] = new SoundEffect
            {
                name = bgmResources[i].name, // name을 오디오 클립의 파일 이름으로 설정
                clip = bgmResources[i],      // clip에 불러온 오디오 클립을 할당
                volume = 1.0f                // 기본 볼륨을 1.0으로 설정
            };
        }

        // 2. Resources/SFX 폴더에서 모든 AudioClip 파일을 불러옵니다. (BGM과 동일한 과정)
        AudioClip[] sfxResources = Resources.LoadAll<AudioClip>("SFX");
        SFXClips = new SoundEffect[sfxResources.Length];
        for (int i = 0; i < sfxResources.Length; i++)
        {
            SFXClips[i] = new SoundEffect
            {
                name = sfxResources[i].name,
                clip = sfxResources[i],
                volume = 1.0f
            };
        }

        // 3. 배열에 정리된 사운드 클립들을 딕셔너리에 추가하여 검색이 용이하도록 만듭니다.
        foreach (var bgm in BGMClips) bgmDict.Add(bgm.name, bgm);
        foreach (var sfx in SFXClips) sfxDict.Add(sfx.name, sfx);
    }

    /// <summary>
    /// BGM의 전체 볼륨을 조절합니다.
    /// </summary>
    /// <param name="volume">설정할 볼륨 크기 (0.0 ~ 1.0)</param>
    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null) bgmSource.volume = volume;
    }

    /// <summary>
    /// SFX의 전체 볼륨을 조절합니다.
    /// </summary>
    /// <param name="volume">설정할 볼륨 크기 (0.0 ~ 1.0)</param>
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null) sfxSource.volume = volume;
    }

    /// <summary>
    /// 지정된 이름의 BGM을 재생합니다.
    /// </summary>
    /// <param name="bgmName">재생할 BGM의 파일 이름</param>
    public void PlayBGM(string bgmName)
    {
        // 딕셔너리에서 이름으로 BGM을 찾습니다.
        if (bgmDict.TryGetValue(bgmName, out SoundEffect sound))
        {
            bgmSource.clip = sound.clip; // BGM 오디오 소스의 클립을 교체
            bgmSource.loop = true;       // BGM은 보통 반복 재생하므로 loop 속성을 true로 설정
            bgmSource.Play();            // BGM 재생
        }
        else Debug.LogWarning("BGM not found: " + bgmName); // 딕셔너리에 해당 이름의 BGM이 없으면 경고 메시지 출력
    }

    /// <summary>
    /// 현재 재생 중인 BGM을 정지합니다.
    /// </summary>
    public void StopBGM() => bgmSource.Stop();

    /// <summary>
    /// 지정된 이름의 SFX를 2D 공간에서 한 번 재생합니다. (UI 사운드 등)
    /// </summary>
    /// <param name="sfxName">재생할 SFX의 파일 이름</param>
    public void PlaySFX(string sfxName)
    {
        if (sfxDict.TryGetValue(sfxName, out SoundEffect sound))
        {
            // PlayOneShot: 현재 재생 중인 사운드를 멈추지 않고 새로운 사운드를 겹쳐서 재생합니다. 효과음에 적합합니다.
            sfxSource.PlayOneShot(sound.clip, sound.volume);
        }
        else Debug.LogWarning("SFX not found: " + sfxName);
    }

    /// <summary>
    /// 지정된 이름의 SFX를 3D 공간의 특정 위치에서 재생합니다. (총소리, 발소리 등)
    /// </summary>
    /// <param name="sfxName">재생할 SFX의 파일 이름</param>
    /// <param name="position">사운드가 재생될 월드 좌표</param>
    public void PlaySFXAtPoint(string sfxName, Vector3 position)
    {
        if (sfxDict.TryGetValue(sfxName, out SoundEffect sound))
        {
            // PlayClipAtPoint: 지정된 위치에 임시 오디오 소스를 생성하여 사운드를 재생하고, 끝나면 자동으로 파괴합니다.
            // 3D positional audio에 사용됩니다.
            AudioSource.PlayClipAtPoint(sound.clip, position, sound.volume * sfxSource.volume);
        }
        else Debug.LogWarning("SFX not found: " + sfxName);
    }
}