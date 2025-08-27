using UnityEngine;

public class playSound : MonoBehaviour
{
    [Header("재생할 사운드 이름 (SoundManager에 등록된 이름)")]
    // 인스펙터에서 재생할 BGM 파일의 이름을 입력받습니다.
    public string bgmNameToPlay;

    // 인스펙터에서 재생할 SFX 파일의 이름을 입력받습니다.
    public string sfxNameToPlay;

    // 인스펙터에서 재생할 3D SFX 파일의 이름을 입력받습니다.
    public string sfx3DNameToPlay;


    // SoundManager의 인스턴스를 저장해 둘 변수
    private SoundManager soundManager;

    void Start()
    {
        // Start 시점에 SoundManager 인스턴스를 찾아 변수에 할당
        soundManager = SoundManager.instance;

        // 만약 SoundManager를 찾지 못했다면, 에러 메시지를 출력
        if (soundManager == null)
        {
            Debug.LogError("[Error] SoundManager does not exist at scene");
        }
    }

    void Update()
    {
        // 배경음 재생
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // 인스펙터에 이름이 지정되었을 경우에만 BGM을 재생합니다.
            if (!string.IsNullOrEmpty(bgmNameToPlay))
            {
                SoundManager.instance.PlayBGM(bgmNameToPlay);
            }
        }
        // 배경음 정지
        else if (Input.GetKeyDown(KeyCode.W))
        {
            SoundManager.instance.StopBGM();
        }
        // 효과음 재생
        else if (Input.GetKeyDown(KeyCode.E))
        {
            // 인스펙터에 이름이 지정되었을 경우에만 SFX를 재생합니다.
            if (!string.IsNullOrEmpty(sfxNameToPlay))
            {
                SoundManager.instance.PlaySFX(sfxNameToPlay);
                Debug.Log("Play SFX: " + sfxNameToPlay);
            }
        }
        // 3D 효과음 재생
        else if (Input.GetKeyDown(KeyCode.R))
        {
            // 인스펙터에 이름이 지정되었을 경우에만 3D SFX를 재생합니다.
            if (!string.IsNullOrEmpty(sfx3DNameToPlay))
            {
                SoundManager.instance.PlaySFXAtPoint(sfx3DNameToPlay, transform.position);
            }
        }
    }
}