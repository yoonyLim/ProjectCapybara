using UnityEngine;
using System.Collections.Generic; // List를 사용하기 위해 추가

public class PlaySound : MonoBehaviour
{
    [Header("재생할 사운드 목록")]
    // 여러 BGM 이름을 담을 수 있는 리스트로 변경
    public List<string> bgmNameList = new List<string>();

    // 여러 SFX 이름을 담을 수 있는 리스트로 변경
    public List<string> sfxNameList = new List<string>();

    // 여러 3D SFX 이름을 담을 수 있는 리스트로 변경
    public List<string> sfx3DNameList = new List<string>();

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
        // 배경음 재생 (Q 키)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // 리스트에 BGM이 하나 이상 등록되어 있을 경우
            if (bgmNameList.Count > 0)
            {
                // 0부터 리스트 개수 -1 사이의 랜덤한 인덱스를 뽑음
                int randomIndex = Random.Range(0, bgmNameList.Count);
                // 해당 인덱스의 BGM 이름을 가져옴
                string randomBgmName = bgmNameList[randomIndex];
                // 랜덤하게 선택된 BGM을 재생
                soundManager.PlayBGM(randomBgmName);
                Debug.Log("Play BGM: " + randomBgmName);
            }
        }
        // 배경음 정지 (W 키)
        else if (Input.GetKeyDown(KeyCode.W))
        {
            soundManager.StopBGM();
        }
        // 2D 효과음 재생 (E 키)
        else if (Input.GetKeyDown(KeyCode.E))
        {
            // 리스트에 SFX가 하나 이상 등록되어 있을 경우
            if (sfxNameList.Count > 0)
            {
                int randomIndex = Random.Range(0, sfxNameList.Count);
                string randomSfxName = sfxNameList[randomIndex];
                soundManager.PlaySFX(randomSfxName);
                Debug.Log("Play SFX: " + randomSfxName);
            }
        }
        // 3D 효과음 재생 (R 키)
        else if (Input.GetKeyDown(KeyCode.R))
        {
            // 리스트에 3D SFX가 하나 이상 등록되어 있을 경우
            if (sfx3DNameList.Count > 0)
            {
                int randomIndex = Random.Range(0, sfx3DNameList.Count);
                string randomSfx3dName = sfx3DNameList[randomIndex];
                soundManager.PlaySFXAtPoint(randomSfx3dName, transform.position);
                Debug.Log("Play 3D SFX at point: " + randomSfx3dName);
            }
        }
    }
}