using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.XInput;

// Explicit opt-in test. Normal launches do not create a virtual controller.
public class CodexCapybaraPadProbe : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-capybaraPadProbe") < 0) return;
        var probe = new GameObject("Capybara controller diagnostic");
        DontDestroyOnLoad(probe);
        probe.AddComponent<CodexCapybaraPadProbe>();
    }

    private IEnumerator Start()
    {
        // A hidden batch test has no focused Game View/window. This override is
        // restricted to the explicit diagnostic flag and synthetic input.
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
        yield return new WaitForSecondsRealtime(4);
        var menu = UIManager.instance;
        var system = EventSystem.current;
        var module = system?.GetComponent<InputSystemUIInputModule>();
        if (menu == null || system == null || module == null)
        { Finish(false, "Menu input components missing"); yield break; }
        Debug.Log($"CAPY_PAD_INITIAL: selected={system.currentSelectedGameObject?.name}; focused={system.isFocused}; moveEnabled={module.move.action.enabled}; submitEnabled={module.submit.action.enabled}; devices={string.Join(",", InputSystem.devices)}");
        var originalSelection = system.currentSelectedGameObject;
        var controller = InputSystem.AddDevice<XInputController>("Capybara diagnostic Xbox controller");
        try
        {
            // Reproduce selection lost after a background click/window refocus.
            system.SetSelectedGameObject(null);
            InputSystem.QueueStateEvent(controller, new GamepadState { leftStick = Vector2.down });
            yield return new WaitForSecondsRealtime(0.15f);
            InputSystem.QueueStateEvent(controller, new GamepadState());
            yield return new WaitForSecondsRealtime(0.1f);
            Debug.Log($"CAPY_PAD_RECOVERY: selected={system.currentSelectedGameObject?.name ?? "NULL"}");
            bool expectOriginalBug = Array.IndexOf(Environment.GetCommandLineArgs(), "-capybaraExpectSelectionBug") >= 0;
            if (expectOriginalBug)
            {
                InputSystem.QueueStateEvent(controller, new GamepadState().WithButton(GamepadButton.East));
                yield return new WaitForSecondsRealtime(0.15f);
                InputSystem.QueueStateEvent(controller, new GamepadState());
                yield return new WaitForSecondsRealtime(0.1f);
                if (system.currentSelectedGameObject != null || !menu.IsMenuUIOpen)
                { Finish(false, "Expected original selection-loss bug did not reproduce"); yield break; }
                Debug.Log("CAPY_PAD_BASELINE: lost selection blocks stick and B");
                system.SetSelectedGameObject(originalSelection);
            }
            else
            {
                if (system.currentSelectedGameObject == null)
                { Finish(false, "Controller navigation failed to restore selection"); yield break; }
                // Clear selection again: B alone must also recover and submit.
                system.SetSelectedGameObject(null);
            }
            InputSystem.QueueStateEvent(controller, new GamepadState().WithButton(GamepadButton.East));
            yield return new WaitForSecondsRealtime(0.4f);
            InputSystem.QueueStateEvent(controller, new GamepadState());
            if (menu.IsMenuUIOpen)
            { Finish(false, "B did not start the game"); yield break; }
            Debug.Log("CAPY_PAD_SUBMIT: B started gameplay");
            Finish(true, expectOriginalBug ? "original bug reproduced; selected button accepts Xbox B" : "selection recovery and Xbox B start passed");
        }
        finally { InputSystem.RemoveDevice(controller); }
    }

    private static void Finish(bool passed, string detail)
    {
        Debug.Log($"CAPY_PAD_RESULT: {(passed ? "passed" : "failed")}; {detail}");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.Exit(passed ? 0 : 1);
#else
        Application.Quit(passed ? 0 : 1);
#endif
    }
}
