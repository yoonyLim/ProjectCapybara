using UnityEngine;
using UnityEngine.InputSystem; // Input System을 사용하기 위해 네임스페이스 추가

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI & Input")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PlayerInput playerInput; // PlayerInput 컴포넌트 연결

    private bool isPaused = false;

    void Awake()
    {
        // Singleton
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
    }

    /// <summary>
    /// Input Action Asset의 "Resume" 액션이 호출될 때 실행됩니다.
    /// (PlayerInput 컴포넌트의 Events에서 연결 필요)
    /// </summary>
    public void OnResume(InputValue value)
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

    /// <summary>
    /// 게임을 일시 정지시키는 함수
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // 게임의 시간을 멈춘다
        playerInput.SwitchCurrentActionMap("UI"); // 플레이어의 입력을 "UI" 모드로 전환
        uiManager.OpenPauseMenu(); // UIManager에게 일시 정지 메뉴를 열도록 요청
    }

    /// <summary>
    /// 게임을 다시 재개하는 함수
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // 게임의 시간을 다시 흐르게 한다
        playerInput.SwitchCurrentActionMap("GamePlay"); // 플레이어의 입력을 "GamePlay" 모드로 전환
        uiManager.CloseAllUI(); // UIManager에게 모든 UI를 닫도록 요청
    }
}