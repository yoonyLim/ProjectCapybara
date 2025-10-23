using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool IsPaused { get; private set; } = false;

    void Awake()
    {
        // Singleton 패턴
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 게임 일시 정지
    /// </summary>
    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f; // 게임의 시간을 멈춥니다.
        Debug.Log("Game Paused. TimeScale: 0");
    }

    /// <summary>
    /// 게임 재개
    /// </summary>
    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f; // 게임의 시간을 다시 흐르게 합니다.
        Debug.Log("Game Resumed. TimeScale: 1");
    }
}