using Capybara;
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

    private Dialogue currentActiveDialogue;

    [Header("UI Panels")]
    [SerializeField] private GameObject startMenuUI;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingsMenuUI;
    [SerializeField] private GameObject controlsMenuUI;
    [Tooltip("DialogUI 스크립트가 붙어있는 최상위 캔버스 패널")]
    [SerializeField] private GameObject dialogUI;

    [Header("Sequential How-To-Play (From Menu)")]
    [SerializeField] private GameObject howToPlayUIPage1;
    [SerializeField] private GameObject howToPlayUIPage2;
    [SerializeField] private GameObject howToPlayUIPage3;

    [Header("Level-Specific Tutorials (From Dialogue)")]
    [SerializeField] private GameObject level1TutorialUI;
    [SerializeField] private GameObject level2TutorialUI;
    [SerializeField] private GameObject level3TutorialUI;


    [Header("First Selected Buttons for Controller")]
    [SerializeField] private GameObject startMenuFirstButton;
    [SerializeField] private GameObject pauseMenuFirstButton;
    [SerializeField] private GameObject settingsMenuFirstButton;
    [Tooltip("대화창이 열릴 때 기본으로 선택될 '다음' 버튼")]
    [SerializeField] private GameObject dialogUIFirstButton;
    // (순차적)
    [SerializeField] private GameObject howToPlayPage1_FirstButton;
    [SerializeField] private GameObject howToPlayPage2_FirstButton;
    [SerializeField] private GameObject howToPlayPage3_FirstButton;
    // (레벨별)
    [SerializeField] private GameObject level1Tutorial_FirstButton;
    [SerializeField] private GameObject level2Tutorial_FirstButton;
    [SerializeField] private GameObject level3Tutorial_FirstButton;


    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Ease openEase = Ease.OutBack;
    [SerializeField] private Ease closeEase = Ease.InBack;

    [Header("Title Camera Transition")]
    [SerializeField] private CinemachineCamera vcam_Title;
    [SerializeField] private CinemachineCamera vcam_Player;

    private Stack<GameObject> uiStack = new Stack<GameObject>();
    private Dictionary<GameObject, GameObject> uiFirstButtons = new Dictionary<GameObject, GameObject>();

    public bool IsMenuUIOpen => uiStack.Count > 0;
    public bool IsDialogueActive => currentActiveDialogue != null;

    // [신규] 입력 겹침 방지용 쿨다운 변수
    private bool isInputProcessing = false;

    #endregion

    #region Initialization

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        if (vcam_Title != null) vcam_Title.Priority = 20;
        else Debug.LogError("UIManager: vcam_Title이(가) 할당되지 않았습니다!", this);

        if (vcam_Player != null) vcam_Player.Priority = 10;
        else Debug.LogError("UIManager: vcam_Player가 할당되지 않았습니다!", this);

        // 딕셔너리에 UI 패널과 기본 버튼 등록
        if (startMenuUI != null) uiFirstButtons[startMenuUI] = startMenuFirstButton;
        if (pauseMenuUI != null) uiFirstButtons[pauseMenuUI] = pauseMenuFirstButton;
        if (settingsMenuUI != null) uiFirstButtons[settingsMenuUI] = settingsMenuFirstButton;
        if (dialogUI != null) uiFirstButtons[dialogUI] = dialogUIFirstButton;
        if (howToPlayUIPage1 != null) uiFirstButtons[howToPlayUIPage1] = howToPlayPage1_FirstButton;
        if (howToPlayUIPage2 != null) uiFirstButtons[howToPlayUIPage2] = howToPlayPage2_FirstButton;
        if (howToPlayUIPage3 != null) uiFirstButtons[howToPlayUIPage3] = howToPlayPage3_FirstButton;
        if (level1TutorialUI != null) uiFirstButtons[level1TutorialUI] = level1Tutorial_FirstButton;
        if (level2TutorialUI != null) uiFirstButtons[level2TutorialUI] = level2Tutorial_FirstButton;
        if (level3TutorialUI != null) uiFirstButtons[level3TutorialUI] = level3Tutorial_FirstButton;

        // 시작 시 모든 UI 즉시 닫기
        if (startMenuUI != null) CloseUIImmediately(startMenuUI);
        if (pauseMenuUI != null) CloseUIImmediately(pauseMenuUI);
        if (settingsMenuUI != null) CloseUIImmediately(settingsMenuUI);
        if (controlsMenuUI != null) CloseUIImmediately(controlsMenuUI);
        if (dialogUI != null) CloseUIImmediately(dialogUI);
        if (howToPlayUIPage1 != null) CloseUIImmediately(howToPlayUIPage1);
        if (howToPlayUIPage2 != null) CloseUIImmediately(howToPlayUIPage2);
        if (howToPlayUIPage3 != null) CloseUIImmediately(howToPlayUIPage3);
        if (level1TutorialUI != null) CloseUIImmediately(level1TutorialUI);
        if (level2TutorialUI != null) CloseUIImmediately(level2TutorialUI);
        if (level3TutorialUI != null) CloseUIImmediately(level3TutorialUI);

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

    /// <summary>
    /// UI를 스택에 쌓고, 이전 UI를 숨깁니다.
    /// </summary>
    public void OpenUI(GameObject uiToOpen)
    {
        uiFirstButtons.TryGetValue(uiToOpen, out GameObject firstSelected);

        if (uiStack.Count > 0)
        {
            AnimateClose(uiStack.Peek()); // 이전 UI 숨기기
        }

        uiStack.Push(uiToOpen);
        AnimateOpen(uiToOpen);

        StartCoroutine(SelectFirstButtonAfterFrame(firstSelected));
    }

    /// <summary>
    /// [신규] 현재 UI를 스택에서 닫고(Pop), 새 UI로 교체합니다. (튜토리얼 페이지 넘기기용)
    /// </summary>
    private void ReplaceUI(GameObject uiToOpen)
    {
        uiFirstButtons.TryGetValue(uiToOpen, out GameObject firstSelected);

        if (uiStack.Count > 0)
        {
            AnimateClose(uiStack.Pop());
        }

        uiStack.Push(uiToOpen);
        AnimateOpen(uiToOpen);

        StartCoroutine(SelectFirstButtonAfterFrame(firstSelected));
    }

    /// <summary>
    /// 뒤로가기: 현재 UI를 닫고(Pop), 이전 UI를 다시 켭니다.
    /// </summary>
    public void CloseAndGoBack()
    {
        if (uiStack.Count > 0)
        {
            AnimateClose(uiStack.Pop()); // 현재 UI 닫고 제거
        }

        if (uiStack.Count > 0)
        {
            GameObject nextUI = uiStack.Peek(); // 이전 UI
            AnimateOpen(nextUI); // 이전 UI 다시 켜기

            if (uiFirstButtons.TryGetValue(nextUI, out GameObject firstSelected))
            {
                StartCoroutine(SelectFirstButtonAfterFrame(firstSelected));
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

    // 입력 겹침(bleed-through) 방지를 위해 한 프레임 대기
    private IEnumerator SelectFirstButtonAfterFrame(GameObject firstSelected)
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    // [신규] 입력 겹침 방지용 쿨다운 코루틴
    private IEnumerator InputCooldown()
    {
        isInputProcessing = true;
        yield return null; // 한 프레임 대기
        isInputProcessing = false;
    }

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

        if (vcam_Title != null && vcam_Player != null)
        {
            vcam_Player.Priority = 30;
        }
        else
        {
            Debug.LogWarning("카메라 전환에 필요한 Virtual Camera가 UIManager에 할당되지 않았습니다.", this);
        }
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
    public void StartDialogue(Dialogue dialogueComponent)
    {
        if (IsMenuUIOpen) return;
        currentActiveDialogue = dialogueComponent;
        OpenUI(dialogUI);
        inputReader.EnableUIActionInputs();
    }

    public void EndDialogue(int tutorialLevel)
    {
        currentActiveDialogue = null;

        GameObject tutorialToOpen = null;
        if (tutorialLevel == 1) tutorialToOpen = level1TutorialUI;
        else if (tutorialLevel == 2) tutorialToOpen = level2TutorialUI;
        else if (tutorialLevel == 3) tutorialToOpen = level3TutorialUI;

        if (tutorialToOpen != null)
        {
            ReplaceUI(tutorialToOpen);
        }
        else
        {
            CloseAllUI();
            inputReader.EnableGamePlayActionInputs();
        }
    }
    #endregion

    #region How To Play UI Functions (Sequential)

    public void OpenHowToPlayPage2()
    {
        ReplaceUI(howToPlayUIPage2);
    }

    public void OpenHowToPlayPage3()
    {
        ReplaceUI(howToPlayUIPage3);
    }

    public void CloseHowToPlay()
    {
        CloseAndGoBack();
    }
    #endregion

    #region Level-Specific Tutorial Functions

    public void CloseLevelTutorial()
    {
        CloseAllUI();
        inputReader.EnableGamePlayActionInputs();
        InteractionComponent.EndInteraction();
    }

    #endregion

    #region Input Event Handlers

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
    /// [수정됨] 입력 겹침 방지 쿨다운 추가
    /// </summary>
    private void HandlePauseEvent()
    {
        if (isInputProcessing) return; // 쿨다운 중이면 반환
        StartCoroutine(InputCooldown()); // 쿨다운 시작

        // 1. 게임 플레이 중일 때
        if (!IsMenuUIOpen && !IsDialogueActive)
        {
            OpenPauseMenu(true);
        }
        // 2. 대화 중일 때
        else if (IsDialogueActive)
        {
            if (uiStack.Count > 0 && uiStack.Peek() == pauseMenuUI)
            {
                // 일시정지 중에 다시 누르면 '취소'와 동일하게 동작
                HandleCancelEvent(); // HandleCancelEvent에도 쿨다운이 있으므로 안전
            }
            else
            {
                // 대화창 '위에' 일시정지 메뉴 열기
                OpenPauseMenu(true);
            }
        }
    }

    public void TriggerNextDialogueStep()
    {
        if (currentActiveDialogue != null)
        {
            currentActiveDialogue.AdvanceDialogue();
        }
    }

    /// <summary>
    /// [수정됨] 입력 겹침 방지 쿨다운 추가
    /// </summary>
    private void HandleCancelEvent()
    {
        if (isInputProcessing) return; // 쿨다운 중이면 반환
        StartCoroutine(InputCooldown()); // 쿨다운 시작

        if (!IsMenuUIOpen) return;

        GameObject topUI = uiStack.Peek();

        bool isLevelTutorial = (topUI == level1TutorialUI || topUI == level2TutorialUI || topUI == level3TutorialUI);

        // 1. UI가 2개 이상 겹쳐있을 때
        if (uiStack.Count > 1)
        {
            CloseAndGoBack();

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
            if (topUI == pauseMenuUI)
            {
                ResumeGameFromUI();
            }
            else if (topUI == howToPlayUIPage1)
            {
                ResumeGameFromUI();
            }
            else if (isLevelTutorial)
            {
                CloseLevelTutorial();
            }
        }
    }
    #endregion
}