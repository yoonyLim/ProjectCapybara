using UnityEngine;

public class playSound : MonoBehaviour
{
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
            
            SoundManager.instance.PlayBGM("BGM_A");
        }
        // 배경음 정지
        else if (Input.GetKeyDown(KeyCode.W))
        {

            SoundManager.instance.StopBGM();
        }
        // 효과음 재생
        else if (Input.GetKeyDown(KeyCode.E))
        {
            SoundManager.instance.PlaySFX("SFX_A");
        }
        // 3D 효과음 재생
        else if (Input.GetKeyDown(KeyCode.R))
        {
            SoundManager.instance.PlaySFX("SFX_B");
        }
    }
}
