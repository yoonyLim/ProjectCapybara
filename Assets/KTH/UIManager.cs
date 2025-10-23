using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Capybara; // InputReader 참조

/// <summary>
/// UI 패널과 대화 상태를 총괄하는 매니저입니다.
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

    // [중요] 대화 캔버스 UI를 다시 추가합니다.
    [Tooltip("DialogUI 스크립트가 붙어있는 최상위 캔버스 패널")]
    [SerializeField] private GameObject dialogUI;

    [Header("First Selected Buttons for Controller")]
    [SerializeField] private GameObject startMenuFirstButton;
    [SerializeField] private GameObject pauseMenuFirstButton;
    [SerializeField] private GameObject settingsMenuFirstButton;
    [SerializeField] private GameObject controlsMenuFirstButton;

    // [중요] 대화 캔버스의 '다음' 버튼을 추가합니다.
    [Tooltip("대화창이 열릴 때 기본으로 선택될 '다음' 버튼")]
    [SerializeField] private GameObject dialogUIFirstButton;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Ease openEase = Ease.OutBack;
    [SerializeField] private Ease closeEase = Ease.InBack;

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
        // 딕셔너리에 UI 패널과 기본 버튼 등록
        if (startMenuUI != null) uiFirstButtons[startMenuUI] = startMenuFirstButton;
        if (pauseMenuUI != null) uiFirstButtons[pauseMenuUI] = pauseMenuFirstButton;
        if (settingsMenuUI != null) uiFirstButtons[settingsMenuUI] = settingsMenuFirstButton;
        if (controlsMenuUI != null) uiFirstButtons[controlsMenuUI] = controlsMenuFirstButton;

        // [중요] 대화 UI 등록
        if (dialogUI != null) uiFirstButtons[dialogUI] = dialogUIFirstButton;

        // 시작 시 모든 UI 즉시 닫기
        if (startMenuUI != null) CloseUIImmediately(startMenuUI);
        if (pauseMenuUI != null) CloseUIImmediately(pauseMenuUI);
        if (settingsMenuUI != null) CloseUIImmediately(settingsMenuUI);
        if (controlsMenuUI != null) CloseUIImmediately(controlsMenuUI);

        // [중요] 대화 UI도 닫기
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

    private void OnEnable()
    {
        if (inputReader != null)
        {
            inputReader.PauseEvent += HandlePauseEvent;
            inputReader.CancelEvent += HandleCancelEvent;

            // [중요] '다음' 이벤트(Submit)를 public 함수로 연결
            inputReader.SubmitEvent += TriggerNextDialogueStep;
        }
    }

    private void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.PauseEvent -= HandlePauseEvent;
            inputReader.CancelEvent -= HandleCancelEvent;
            inputReader.SubmitEvent -= TriggerNextDialogueStep; // 구독 해제
        }
    }

    #endregion

    // Core UI Logic (OpenUI, CloseAndGoBack, CloseAllUI)
    // Animation (AnimateOpen, AnimateClose, CloseUIImmediately)
    // Public UI Button Functions (OpenSettingMenu, StartGame, ResumeGameFromUI)
    // ... (이전과 동일한 함수들) ...

    #region Core UI Logic (Panels)

    /// <summary>
    /// UI 패널을 엽니다 (애니메이션, 스택 관리, 컨트롤러 포커스 설정 포함)
    /// </summary>
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

    /// <summary>
    /// 현재 UI 패널을 닫고 스택의 이전 UI 패널로 돌아갑니다.
    /// </summary>
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

    /// <summary>
    /// 스택에 있는 모든 UI 패널을 닫습니다. (게임 시작 또는 재개 시 사용)
    /// </summary>
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

    public void StartGame()
    {
        CloseAllUI();
        inputReader.EnableGamePlayActionInputs();

        if (GameManager.instance != null)
        {
            GameManager.instance.ResumeGame();
        }
    }

    /// <summary>
    /// 'Resume' (메뉴 UI 또는 대화 종료 시)
    /// </summary>
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
    /// Dialogue.cs가 호출: "대화 시작"
    /// UIManager는 대화 캔버스를 열고 게임 상태를 변경합니다.
    /// </summary>
    public void StartDialogue(Dialogue dialogueComponent)
    {
        if (IsMenuUIOpen) return; // 메뉴가 열려있으면 대화 시작 안 함

        currentActiveDialogue = dialogueComponent; // 현재 활성 대화(로직)로 등록

        // [중요] 캔버스 대화창을 엽니다.
        OpenUI(dialogUI);

        inputReader.EnableUIActionInputs(); // 입력 모드를 UI로 변경

        if (GameManager.instance != null)
        {
            GameManager.instance.PauseGame(); // 게임 시간 정지
        }
    }

    /// <summary>
    /// Dialogue.cs가 호출: "대화 종료"
    /// UIManager는 대화 캔버스를 닫고 게임 상태를 복구합니다.
    /// </summary>
    public void EndDialogue()
    {
        currentActiveDialogue = null; // 활성 대화 등록 해제

        // [중요] ResumeGameFromUI를 호출하여 캔버스를 닫고 게임을 재개합니다.
        ResumeGameFromUI();
    }

    #endregion

    #region Input Event Handlers

    private void HandlePauseEvent()
    {
        // 메뉴X, 대화X 일 때만 일시정지 메뉴 열기
        if (!IsMenuUIOpen && !IsDialogueActive)
        {
            OpenUI(pauseMenuUI);
            inputReader.EnableUIActionInputs();

            if (GameManager.instance != null)
            {
                GameManager.instance.PauseGame();
            }
        }
    }

    /// <summary>
    /// [PUBLIC] '확인/다음' 입력 (InputReader 또는 UI 버튼 클릭)을 처리합니다.
    /// </summary>
    public void TriggerNextDialogueStep()
    {
        // 활성화된 대화(로직)가 있다면
        if (currentActiveDialogue != null)
        {
            // Dialogue.cs에게 대화를 넘기라고 명령
            currentActiveDialogue.AdvanceDialogue();
        }
    }

    private void HandleCancelEvent()
    {
        if (!IsMenuUIOpen) return; // 메뉴가 열려있지 않으면 무시

        GameObject topUI = uiStack.Peek();

        if (uiStack.Count > 1)
        {
            CloseAndGoBack();
        }
        else
        {
            if (topUI == pauseMenuUI)
            {
                ResumeGameFromUI();
            }
        }
    }

    #endregion
}