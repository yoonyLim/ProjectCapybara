using UnityEngine;
using System.Collections.Generic; // Stack을 사용하기 위해 꼭 필요합니다!

public class UIManager : MonoBehaviour
{
    // ... 기존 변수 선언은 그대로 둡니다 ...
    [SerializeField] private GameObject startMenuUI;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingsMenuUI;
    [SerializeField] private GameObject controlsMenuUI;

    // 열려있는 UI들의 순서를 기억하기 위한 스택
    private Stack<GameObject> uiStack = new Stack<GameObject>();

    void Start()
    {
        // 모든 UI를 끈 상태로 시작
        if (startMenuUI != null) startMenuUI.SetActive(false);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);
        if (controlsMenuUI != null) controlsMenuUI.SetActive(false);

        // 메인 메뉴 씬이라면 시작 UI를 켠다
        if (startMenuUI != null)
        {
            OpenUI(startMenuUI);
        }
    }

    // 새로운 UI를 여는 함수 (스택에 쌓는 방식)
    public void OpenUI(GameObject uiToOpen)
    {
        // 1. 만약 현재 켜져 있는 UI가 있다면, 그 UI를 끈다.
        if (uiStack.Count > 0)
        {
            GameObject topUI = uiStack.Peek(); // 가장 위에 있는 UI를 확인
            topUI.SetActive(false);
        }

        // 2. 새로 열 UI를 스택의 가장 위에 추가하고 화면에 켠다.
        uiStack.Push(uiToOpen);
        uiToOpen.SetActive(true);
    }

    public void OpenPauseMenu()
    {
        OpenUI(pauseMenuUI);
    }

    public void OpenSettingMenu()
    {
        OpenUI(settingsMenuUI);
    }


    // UI를 닫고 이전 UI로 돌아가는 함수 (뒤로가기 버튼용)
    public void CloseAndGoBack()
    {
        // 1. 현재 UI(스택의 가장 위)를 끄고 스택에서 제거한다.
        if (uiStack.Count > 0)
        {
            GameObject topUI = uiStack.Pop();
            topUI.SetActive(false);
        }

        // 2. 스택에 아직 UI가 남아있다면, 그 다음 UI를 다시 켠다.
        if (uiStack.Count > 0)
        {
            GameObject nextUI = uiStack.Peek();
            nextUI.SetActive(true);
        }
    }

    // 모든 UI를 닫는 함수 (게임으로 복귀할 때 사용)
    public void CloseAllUI()
    {
        // 스택이 빌 때까지 모든 UI를 끄고 스택에서 제거한다.
        while (uiStack.Count > 0)
        {
            GameObject ui = uiStack.Pop();
            ui.SetActive(false);
        }
    }
}