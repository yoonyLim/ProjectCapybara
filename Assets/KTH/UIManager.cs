using Capybara; // InputReader 참조
using Unity.Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI 패널과 대화 상태, 그리고 Cinemachine 카메라 전환을 총괄하는 매니저입니다.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    #region Fields and Properties

    [Header("Input")]
    [SerializeField] private CapybaraInputReader inputReader;

    /// <summary>
    /// 현재 대화 로직을 담당하는 NPC의 Dialogue 컴포넌트
    /// </summary>
    private Dialogue currentActiveDialogue;

    [Header("UI Panels")]
    [SerializeField] private GameObject startMenuUI;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingsMenuUI;
    [SerializeField] private GameObject controlsMenuUI;

    [Tooltip("DialogUI 스크립트가 붙어있는 최상위 캔버스 패널")]
    [SerializeField] private GameObject dialogUI;

    [Header("First Selected Buttons for Controller")]
    [SerializeField] private GameObject startMenuFirstButton;
    [SerializeField] private GameObject pauseMenuFirstButton;
    [SerializeField] private GameObject settingsMenuFirstButton;

    [Tooltip("대화창이 열릴 때 기본으로 선택될 '다음' 버튼")]
    [SerializeField] private GameObject dialogUIFirstButton;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Ease openEase = Ease.OutBack;
    [SerializeField] private Ease closeEase = Ease.InBack;


    // [수정됨] Camera 대신 CinemachineVirtualCamera를 참조합니다.
    [Header("Title Camera Transition")]
    [Tooltip("1단계: 타이틀 씬의 Cinemachine 가상 카메라 (Priority 20)")]
    [SerializeField] private CinemachineCamera vcam_Title;

    [Tooltip("2단계: 플레이어를 따라다니는 Cinemachine 가상 카메라 (Priority 10)")]
    [SerializeField] private CinemachineCamera vcam_Player;

    // [삭제] transitionDuration 필드 삭제 (Cinemachine Brain이 처리)

    private Stack<GameObject> uiStack = new Stack<GameObject>();
    private Dictionary<GameObject, GameObject> uiFirstButtons = new Dictionary<GameObject, GameObject>();

    public bool IsMenuUIOpen => uiStack.Count > 0;
    public bool IsDialogueActive => currentActiveDialogue != null;

    #endregion

    #region Initialization

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        // [수정됨] 카메라 활성화 대신 '우선순위(Priority)'로 초기 상태를 설정합니다.
        // 1. 타이틀 카메라는 높은 우선순위 (예: 20)
        if (vcam_Title != null)
        {
            vcam_Title.Priority = 20;
        }
        else
        {
            Debug.LogError("UIManager: vcam_Title이(가) 할당되지 않았습니다!", this);
        }

        // 2. 플레이어 카메라는 낮은 우선순위 (예: 10)
        if (vcam_Player != null)
        {
            vcam_Player.Priority = 10;
        }
        else
        {
            Debug.LogError("UIManager: vcam_Player가 할당되지 않았습니다!", this);
        }

        // --- (기존 UI 초기화 로직은 그대로 유지) ---
        // 딕셔너리에 UI 패널과 기본 버튼 등록
        if (startMenuUI != null) uiFirstButtons[startMenuUI] = startMenuFirstButton;
        if (pauseMenuUI != null) uiFirstButtons[pauseMenuUI] = pauseMenuFirstButton;
        if (settingsMenuUI != null) uiFirstButtons[settingsMenuUI] = settingsMenuFirstButton;
        if (dialogUI != null) uiFirstButtons[dialogUI] = dialogUIFirstButton;

        // 시작 시 모든 UI 즉시 닫기
        if (startMenuUI != null) CloseUIImmediately(startMenuUI);
        if (pauseMenuUI != null) CloseUIImmediately(pauseMenuUI);
        if (settingsMenuUI != null) CloseUIImmediately(settingsMenuUI);
        if (controlsMenuUI != null) CloseUIImmediately(controlsMenuUI);
        if (dialogUI != null) CloseUIImmediately(dialogUI);

        // 시작 메뉴 열기
        if (startMenuUI != null)
        {
            OpenUI(startMenuUI);
            inputReader.EnableUIActionInputs();
        }
    }

    #endregion

    #region Event Subscription
    // ... (OnEnable, OnDisable - 수정 없음) ...
    private void OnEnable()
    {
        if (inputReader != null)
        {
            inputReader.PauseEvent += HandlePauseEvent;
            inputReader.CancelEvent += HandleCancelEvent;
            inputReader.SubmitEvent += TriggerNextDialogueStep;
        }
    }

    private void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.PauseEvent -= HandlePauseEvent;
            inputReader.CancelEvent -= HandleCancelEvent;
            inputReader.SubmitEvent -= TriggerNextDialogueStep;
        }
    }
    #endregion

    #region Core UI Logic (Panels)
    // ... (OpenUI, CloseAndGoBack, CloseAllUI - 수정 없음) ...
    public void OpenUI(GameObject uiToOpen)
    {
        uiFirstButtons.TryGetValue(uiToOpen, out GameObject firstSelected);

        if (uiStack.Count > 0)
        {
            AnimateClose(uiStack.Peek());
        }

        uiStack.Push(uiToOpen);
        AnimateOpen(uiToOpen);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }
    public void CloseAndGoBack()
    {
        if (uiStack.Count > 0)
        {
            AnimateClose(uiStack.Pop());
        }

        if (uiStack.Count > 0)
        {
            GameObject nextUI = uiStack.Peek();
            AnimateOpen(nextUI);

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
            AnimateClose(uiStack.Pop());
        }
        EventSystem.current.SetSelectedGameObject(null);
    }
    #endregion

    #region Animation & Public Functions (Buttons)
    // ... (AnimateOpen, AnimateClose, CloseUIImmediately - 수정 없음) ...
    private void AnimateOpen(GameObject uiObject)
    {
        CanvasGroup canvasGroup = uiObject.GetComponent<CanvasGroup>() ?? uiObject.AddComponent<CanvasGroup>();
        uiObject.SetActive(true);
        canvasGroup.alpha = 0f;
        uiObject.transform.localScale = Vector3.one * 0.9f;

        canvasGroup.DOFade(1f, animationDuration).SetUpdate(true);
        uiObject.transform.DOScale(1f, animationDuration).SetEase(openEase).SetUpdate(true);

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void AnimateClose(GameObject uiObject)
    {
        CanvasGroup canvasGroup = uiObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null) return;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        canvasGroup.DOFade(0f, animationDuration).SetUpdate(true);
        uiObject.transform.DOScale(0.9f, animationDuration).SetEase(closeEase).SetUpdate(true)
            .OnComplete(() => uiObject.SetActive(false));
    }

    private void CloseUIImmediately(GameObject uiObject)
    {
        CanvasGroup canvasGroup = uiObject.GetComponent<CanvasGroup>() ?? uiObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        uiObject.SetActive(false);
    }

    // --- Public UI Button Functions ---

    public void OpenSettingMenu() => OpenUI(settingsMenuUI);
    public void OpenControlsMenu() => OpenUI(controlsMenuUI);

    /// <summary>
    /// 'Start Game' 버튼 (시작 메뉴용)
    /// [수정됨] Cinemachine 우선순위를 변경하여 카메라 전환을 시작합니다.
    /// </summary>
    public void StartGame()
    {
        // 1. 기존 로직: UI 닫고 입력 모드 변경
        CloseAllUI();
        inputReader.EnableGamePlayActionInputs();

        if (GameManager.instance != null)
        {
            GameManager.instance.ResumeGame();
        }

        // 2. [수정됨] Cinemachine 카메라 우선순위를 변경하여 전환을 시작합니다.
        if (vcam_Title != null && vcam_Player != null)
        {
            // Player 카메라의 우선순위를 Title(20)보다 높게 설정합니다.
            vcam_Player.Priority = 30;
            // (vcam_Title의 우선순위를 낮춰도 동일하게 작동합니다)
            // vcam_Title.Priority = 5;
        }
        else
        {
            Debug.LogWarning("카메라 전환에 필요한 Virtual Camera가 UIManager에 할당되지 않았습니다.", this);
        }

        // [삭제] StartCoroutine(StartCameraTransition()); 삭제
    }

    public void ResumeGameFromUI()
    {
        CloseAllUI();
        inputReader.EnableGamePlayActionInputs();

        if (GameManager.instance != null)
        {
            GameManager.instance.ResumeGame();
        }
    }

    #endregion

    #region Public Dialogue Methods (Canvas)
    /// <summary>
    /// 대화를 시작합니다. (InteractionComponent 또는 DialogueTester에서 호출)
    /// </summary>
    public void StartDialogue(Dialogue dialogueComponent)
    {
        if (IsMenuUIOpen) return;

        currentActiveDialogue = dialogueComponent;

        OpenUI(dialogUI);

        inputReader.EnableUIActionInputs();

    }

    /// <summary>
    /// 대화를 종료합니다.
    /// </summary>
    public void EndDialogue()
    {
        currentActiveDialogue = null;

        CloseAllUI();
        inputReader.EnableGamePlayActionInputs();
    }
    #endregion

    #region Input Event Handlers

    /// <summary>
    /// [신규] 일시정지 메뉴를 열고, 필요시 게임 시간을 멈추는 헬퍼 함수
    /// </summary>
    private void OpenPauseMenu(bool pauseGame)
    {
        OpenUI(pauseMenuUI);
        inputReader.EnableUIActionInputs();

        if (pauseGame && GameManager.instance != null)
        {
            GameManager.instance.PauseGame();
        }
    }

    /// <summary>
    /// [수정됨] 일시정지 이벤트 핸들러
    /// </summary>
    private void HandlePauseEvent()
    {
        // 1. 게임 플레이 중일 때 (기본)
        if (!IsMenuUIOpen && !IsDialogueActive)
        {
            // 일시정지 메뉴를 열고 게임을 멈춥니다.
            OpenPauseMenu(true);
        }
        // 2. 대화 중일 때
        else if (IsDialogueActive)
        {
            // 2-1. 대화 중에 이미 일시정지 메뉴가 열려있다면 (Stack: [Dialog, Pause])
            if (uiStack.Peek() == pauseMenuUI)
            {
                // 일시정지 버튼을 다시 누르면 '취소'와 동일하게 동작 (대화로 복귀)
                HandleCancelEvent();
            }
            // 2-2. 대화만 하고 있다면 (Stack: [Dialog])
            else
            {
                // 일시정지 메뉴를 대화창 '위에' 열고 게임을 멈춥니다.
                OpenPauseMenu(true);
            }
        }
        // 3. (그 외) 메인 메뉴, 설정창 등에서는 '일시정지'가 동작하지 않습니다.
    }

    public void TriggerNextDialogueStep()
    {
        if (currentActiveDialogue != null)
        {
            currentActiveDialogue.AdvanceDialogue();
        }
    }

    /// <summary>
    /// [수정됨] 취소(뒤로가기) 이벤트 핸들러
    /// </summary>
    private void HandleCancelEvent()
    {
        if (!IsMenuUIOpen) return;

        GameObject topUI = uiStack.Peek();

        // 1. UI가 2개 이상 겹쳐있을 때 (예: [Start, Settings] 또는 [Dialog, Pause])
        if (uiStack.Count > 1)
        {
            // 뒤로가기 (이전 UI로 복귀)
            CloseAndGoBack();

            // [중요] 만약 닫힌 UI가 '일시정지 메뉴'였다면,
            // 게임 시간을 다시 흐르게 합니다. (대화창으로 복귀하므로)
            if (topUI == pauseMenuUI)
            {
                if (GameManager.instance != null)
                {
                    GameManager.instance.ResumeGame();
                }
            }
        }
        // 2. UI가 1개만 있을 때
        else
        {
            // 2-1. 게임 플레이 중 열었던 일시정지 메뉴라면
            if (topUI == pauseMenuUI)
            {
                // 게임으로 복귀합니다. (이 함수는 CloseAllUI와 ResumeGame을 포함)
                ResumeGameFromUI();
            }
            // 2-2. (그 외) 시작 메뉴 등에서는 '취소'가 동작하지 않습니다.
        }
    }
    #endregion

    // [삭제] private IEnumerator StartCameraTransition() 코루틴 전체를 삭제합니다.
}
