using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    public bool IsPaused { get; private set; } = false;
    
    void Awake()
    {
        // Singleton ����
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

    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            FindLoadingManager(0);
        } 
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            FindLoadingManager(1);
        } 
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            FindLoadingManager(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            FindLoadingManager(3);
        }
    }

    void FindLoadingManager(int sceneIndex)
    { 
        FindFirstObjectByType<LoadingManager>().LoadScene(sceneIndex);
    }

    /// <summary>
    /// ���� �Ͻ� ����
    /// </summary>
    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f; // ������ �ð��� ����ϴ�.
        Debug.Log("Game Paused. TimeScale: 0");
    }

    /// <summary>
    /// ���� �簳
    /// </summary>
    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f; // ������ �ð��� �ٽ� �帣�� �մϴ�.
        Debug.Log("Game Resumed. TimeScale: 1");
    }
}