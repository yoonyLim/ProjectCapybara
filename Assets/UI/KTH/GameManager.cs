using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    // UIManager를 연결할 변수
    [SerializeField] private UIManager uiManager;

    // 게임이 일시 정지 상태인지 확인하는 변수
    private bool isPaused = false;

    void Update()
    {
        // ESC 키가 눌렸는지 확인
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // 게임을 일시 정지시키는 함수
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // 게임의 시간을 멈춘다

        // UIManager에게 일시 정지 메뉴를 열어달라고 '요청'만 합니다.
        uiManager.OpenPauseMenu();
    }

    // 게임을 다시 재개하는 함수
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // 게임의 시간을 다시 흐르게 한다

        // UIManager에게 현재 열려있는 UI를 닫아달라고 요청!
        uiManager.CloseAndGoBack();
    }
}