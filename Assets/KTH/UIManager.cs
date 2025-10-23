using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Capybara; // Required to reference the CapybaraInputReader

/// <summary>
/// Manages all UI panels, their states, and animations.
/// Handles UI input by subscribing to events from the InputReader.
/// </summary>
public class UIManager : MonoBehaviour
{
    // A static reference to this instance for easy access from other scripts.
    public static UIManager instance;

    #region Fields and Properties

    [Header("Input")]
    [Tooltip("Assign the CapybaraInputReader asset that broadcasts input events.")]
    [SerializeField] private CapybaraInputReader inputReader;

    [Header("UI Panels")]
    [Tooltip("Assign all the UI panels that this manager will control.")]
    [SerializeField] private GameObject startMenuUI;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingsMenuUI;
    [SerializeField] private GameObject controlsMenuUI;

    [Header("First Selected Buttons for Controller")]
    [Tooltip("The default button to be selected when a panel is opened with a controller.")]
    [SerializeField] private GameObject startMenuFirstButton;
    [SerializeField] private GameObject pauseMenuFirstButton;
    [SerializeField] private GameObject settingsMenuFirstButton;
    [SerializeField] private GameObject controlsMenuFirstButton;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Ease openEase = Ease.OutBack;
    [SerializeField] private Ease closeEase = Ease.InBack;

    // A stack to keep track of the order of opened UI panels.
    private Stack<GameObject> uiStack = new Stack<GameObject>();
    // A dictionary to associate each UI panel with its default first button.
    private Dictionary<GameObject, GameObject> uiFirstButtons = new Dictionary<GameObject, GameObject>();

    #endregion

    #region Initialization

    private void Awake()
    {
        // Singleton Pattern: Ensures only one instance of UIManager exists.
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

    private void Start()
    {
        // Register each UI panel and its corresponding first button in the dictionary.
        if (startMenuUI != null) uiFirstButtons[startMenuUI] = startMenuFirstButton;
        if (pauseMenuUI != null) uiFirstButtons[pauseMenuUI] = pauseMenuFirstButton;
        if (settingsMenuUI != null) uiFirstButtons[settingsMenuUI] = settingsMenuFirstButton;
        if (controlsMenuUI != null) uiFirstButtons[controlsMenuUI] = controlsMenuFirstButton;

        // Close all panels immediately at the start to ensure a clean state.
        if (startMenuUI != null) CloseUIImmediately(startMenuUI);
        if (pauseMenuUI != null) CloseUIImmediately(pauseMenuUI);
        if (settingsMenuUI != null) CloseUIImmediately(settingsMenuUI);
        if (controlsMenuUI != null) CloseUIImmediately(controlsMenuUI);

        // Open the start menu when the game begins.
        if (startMenuUI != null)
        {
            OpenUI(startMenuUI);
            // Since the start menu is active, switch the input mode to UI.
            inputReader.EnableUIActionInputs();
        }
    }

    #endregion

    #region Event Subscription

    private void OnEnable()
    {
        if (inputReader != null)
        {
            // Subscribe to the input reader's events.
            // When the PauseEvent is broadcast, the HandlePauseEvent method will be executed.
            inputReader.PauseEvent += HandlePauseEvent;
            // When the CancelEvent is broadcast, the HandleCancelEvent method will be executed.
            inputReader.CancelEvent += HandleCancelEvent;
        }
    }

    private void OnDisable()
    {
        if (inputReader != null)
        {
            // Unsubscribe from events when this object is disabled to prevent memory leaks.
            inputReader.PauseEvent -= HandlePauseEvent;
            inputReader.CancelEvent -= HandleCancelEvent;
        }
    }

    #endregion

    #region Core UI Logic

    /// <summary>
    /// Opens a UI panel with an animation and sets the controller focus.
    /// </summary>
    public void OpenUI(GameObject uiToOpen)
    {
        uiFirstButtons.TryGetValue(uiToOpen, out GameObject firstSelected);

        // If there's already a UI open, close it before opening the new one.
        if (uiStack.Count > 0)
        {
            AnimateClose(uiStack.Peek());
        }

        uiStack.Push(uiToOpen); // Add the new UI to the history stack.
        AnimateOpen(uiToOpen);  // Play the open animation.

        // Set the controller focus to the default button for this UI.
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    /// <summary>
    /// Closes the current UI panel and reopens the previous one from the stack.
    /// </summary>
    public void CloseAndGoBack()
    {
        if (uiStack.Count > 0)
        {
            AnimateClose(uiStack.Pop()); // Close the top UI and remove it from the stack.
        }

        if (uiStack.Count > 0)
        {
            GameObject nextUI = uiStack.Peek(); // Get the previous UI.
            AnimateOpen(nextUI); // Re-open it.

            // Set the controller focus back to the previous UI's default button.
            if (uiFirstButtons.TryGetValue(nextUI, out GameObject firstSelected))
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstSelected);
            }
        }
    }

    /// <summary>
    /// Closes all currently open UI panels.
    /// </summary>
    public void CloseAllUI()
    {
        while (uiStack.Count > 0)
        {
            AnimateClose(uiStack.Pop());
        }
        EventSystem.current.SetSelectedGameObject(null); // Clear controller focus.
    }

    #endregion

    #region Animation & Public Functions

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

    // Public functions to be called by UI buttons' OnClick events.
    public void OpenSettingMenu() => OpenUI(settingsMenuUI);
    public void OpenControlsMenu() => OpenUI(controlsMenuUI);

    /// <summary>
    /// Function to be called by the 'Start Game' button.
    /// </summary>
    public void StartGame()
    {
        CloseAllUI();
        inputReader.EnableGamePlayActionInputs(); // Switch to gameplay input mode.

        if (GameManager.instance != null)
        {
            GameManager.instance.ResumeGame();
        }
    }

    /// <summary>
    /// Function to be called by 'Resume' or 'Back to Game' buttons in the UI.
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

    #region Input Event Handlers

    /// <summary>
    /// Called when the PauseEvent is received from the InputReader.
    /// </summary>
    private void HandlePauseEvent()
    {
        // Only open the pause menu if we are in-game (no other UI is open).
        if (uiStack.Count == 0)
        {
            OpenUI(pauseMenuUI);
            inputReader.EnableUIActionInputs(); // Switch to UI input mode.

            if (GameManager.instance != null)
            {
                GameManager.instance.PauseGame();
            }
        }
    }

    /// <summary>
    /// Called when the CancelEvent is received from the InputReader.
    /// </summary>
    private void HandleCancelEvent()
    {
        if (uiStack.Count == 0) return;

        GameObject topUI = uiStack.Peek();

        // If more than one UI is open (e.g., Settings on top of Pause), go back to the previous one.
        if (uiStack.Count > 1)
        {
            CloseAndGoBack();
        }
        // If only one UI is open...
        else
        {
            // ...and it's the pause menu, close it and return to the game.
            if (topUI == pauseMenuUI)
            {
                ResumeGameFromUI();
            }
            // ...and it's the start menu, do nothing (or implement a 'Quit Game' confirmation).
            else if (topUI == startMenuUI)
            {
                // This is the base menu, so "back" doesn't do anything.
            }
        }
    }

    #endregion
}