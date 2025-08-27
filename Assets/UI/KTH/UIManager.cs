using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using DG.Tweening; // DOTween 네임스페이스 추가!

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("UI Panels")]
    [SerializeField] private GameObject startMenuUI;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingsMenuUI;
    [SerializeField] private GameObject controlsMenuUI;

    [Header("First Selected Buttons for Controller")]
    [SerializeField] private GameObject startMenuFirstButton;
    [SerializeField] private GameObject pauseMenuFirstButton;
    [SerializeField] private GameObject settingsMenuFirstButton;
    [SerializeField] private GameObject controlsMenuFirstButton;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f; // 애니메이션 속도
    [SerializeField] private Ease openEase = Ease.OutBack;   // UI가 나타날 때 Ease 효과
    [SerializeField] private Ease closeEase = Ease.InBack;   // UI가 사라질 때 Ease 효과

    private Stack<GameObject> uiStack = new Stack<GameObject>();
    private Dictionary<GameObject, GameObject> uiFirstButtons = new Dictionary<GameObject, GameObject>();

    void Awake()
    {
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

    void Start()
    {

        if (startMenuUI != null) uiFirstButtons[startMenuUI] = startMenuFirstButton;
        if (pauseMenuUI != null) uiFirstButtons[pauseMenuUI] = pauseMenuFirstButton;
        if (settingsMenuUI != null) uiFirstButtons[settingsMenuUI] = settingsMenuFirstButton;
        if (controlsMenuUI != null) uiFirstButtons[controlsMenuUI] = controlsMenuFirstButton;


        // 모든 UI를 비활성화 상태로 시작
        if (startMenuUI != null) CloseUIImmediately(startMenuUI);
        if (pauseMenuUI != null) CloseUIImmediately(pauseMenuUI);
        if (settingsMenuUI != null) CloseUIImmediately(settingsMenuUI);
        if (controlsMenuUI != null) CloseUIImmediately(controlsMenuUI);

        // 시작 메뉴가 있다면 시작 메뉴를 연다
        if (startMenuUI != null)
        {
            OpenUI(startMenuUI, startMenuFirstButton);
        }
    }

    // =====  핵심 함수 =====

    public void OpenUI(GameObject uiToOpen, GameObject firstSelected)
    {
        // 이전에 열려있던 UI가 있다면 애니메이션으로 닫는다.
        if (uiStack.Count > 0)
        {
            GameObject previousUI = uiStack.Peek();
            AnimateClose(previousUI);
        }

        uiStack.Push(uiToOpen);
        AnimateOpen(uiToOpen); // 애니메이션으로 UI 열기

        // 컨트롤러 포커스 설정
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    public void OpenUI(GameObject uiToOpen)
    {
        // 단순히 firstSelected 자리에 null을 넣어 기존 함수를 호출해줍니다.
        OpenUI(uiToOpen, null);
    }

    public void CloseAndGoBack()
    {
        if (uiStack.Count > 0)
        {
            GameObject topUI = uiStack.Pop();
            AnimateClose(topUI);
        }

        if (uiStack.Count > 0)
        {
            GameObject nextUI = uiStack.Peek();
            AnimateOpen(nextUI);

            // ===== 수정된 부분 =====
            // Dictionary에서 다음 UI에 맞는 첫 버튼을 찾아 포커스를 설정합니다.
            if (uiFirstButtons.TryGetValue(nextUI, out GameObject firstSelected))
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstSelected);
            }
        }
    }

    public void CloseAllUI()
    {
        while (uiStack.Count > 0)
        {
            GameObject ui = uiStack.Pop();
            AnimateClose(ui); // 모든 UI를 애니메이션으로 닫는다.
        }
        EventSystem.current.SetSelectedGameObject(null);
    }

    // ===== 애니메이션 헬퍼 함수 =====

    private void AnimateOpen(GameObject uiObject)
    {
        CanvasGroup canvasGroup = uiObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = uiObject.AddComponent<CanvasGroup>();

        uiObject.SetActive(true);

        // 초기 상태: 투명하고 약간 작은 상태
        canvasGroup.alpha = 0f;
        uiObject.transform.localScale = Vector3.one * 0.9f;

        // 애니메이션 실행
        canvasGroup.DOFade(1f, animationDuration).SetUpdate(true);
        uiObject.transform.DOScale(1f, animationDuration).SetEase(openEase).SetUpdate(true);

        // 애니메이션 시작 시 상호작용 가능하게 설정
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void AnimateClose(GameObject uiObject)
    {
        CanvasGroup canvasGroup = uiObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null) return; // CanvasGroup이 없으면 실행 안함

        // 애니메이션 시작 시 상호작용 불가능하게 설정
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // 애니메이션 실행
        canvasGroup.DOFade(0f, animationDuration).SetUpdate(true);
        uiObject.transform.DOScale(0.9f, animationDuration).SetEase(closeEase).SetUpdate(true)
            .OnComplete(() => {
                uiObject.SetActive(false); // 애니메이션이 끝나면 비활성화
            });
    }

    // 게임 시작 시 UI를 즉시 닫는 함수
    private void CloseUIImmediately(GameObject uiObject)
    {
        CanvasGroup canvasGroup = uiObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = uiObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        uiObject.SetActive(false);
    }

    // ===== 기존 공개 함수 =====

    public void OpenPauseMenu() => OpenUI(pauseMenuUI, pauseMenuFirstButton);
    public void OpenSettingMenu() => OpenUI(settingsMenuUI, settingsMenuFirstButton);
    public void OpenControlsMenu() => OpenUI(controlsMenuUI);

    public void OnCancel(InputValue value)
    {
        if (uiStack.Count > 0)
        {
            // 예: 게임 플레이 중에는 Pause 메뉴를 열고, 메뉴 안에서는 뒤로가기
            // 현재는 스택에 UI가 하나만 있을 때(일시정지 메뉴)는 닫고 게임으로 복귀하도록 구현
            if (uiStack.Count == 1)
            {
                // 게임으로 돌아가는 로직 (예: ResumeGame())
                CloseAllUI();
            }
            else
            {
                CloseAndGoBack();
            }
        }
    }
}