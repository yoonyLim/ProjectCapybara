using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using System.Reflection;

// Explicit opt-in test. Normal launches do not create a virtual controller.
public class CodexCapybaraPadProbe : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-capybaraPadProbe") < 0 &&
            Array.IndexOf(Environment.GetCommandLineArgs(), "-capybaraKeyboardProbe") < 0 &&
            Array.IndexOf(Environment.GetCommandLineArgs(), "-capybaraTitleProbe") < 0) return;
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
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-capybaraTitleProbe") >= 0)
        {
            yield return ProbeTitleInput(menu, system, module);
            yield break;
        }
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-capybaraKeyboardProbe") >= 0)
        {
            var keyboard = InputSystem.AddDevice<Keyboard>("Capybara diagnostic keyboard");
            try
            {
                system.SetSelectedGameObject(null);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
                yield return new WaitForSecondsRealtime(0.4f);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                Finish(!menu.IsMenuUIOpen, "keyboard E start; selected=" + system.currentSelectedGameObject?.name);
            }
            finally { InputSystem.RemoveDevice(keyboard); }
            yield break;
        }
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

    private IEnumerator ProbeTitleInput(UIManager menu, EventSystem system, InputSystemUIInputModule module)
    {
        var args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, "-capybaraTitleCase");
        string scenario = index >= 0 && index + 1 < args.Length ? args[index + 1] : "e";
        if (!menu.IsStartScreenActive) { Finish(false, "Title was not active"); yield break; }

        // Reproduce a missing/disabled UI input path and no selected button.
        // The new title fallback must work independently of both UI assets.
        module.actionsAsset.Disable();
        var reader = (Capybara.CapybaraInputReader)typeof(UIManager).GetField("inputReader", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(menu);
        var actions = (Capybara.CapybaraInput)typeof(Capybara.CapybaraInputReader).GetField("capybaraInput", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(reader);
        actions.Disable();
        system.SetSelectedGameObject(null);

        InputDevice device = null;
        InputDevice connectedPad = null;
        ButtonControl control = null;
        bool expectStart = true;
        if (scenario == "e" || scenario == "enter" || scenario == "space" || scenario == "submenu" || scenario == "connected-keyboard")
        {
            var keyboard = InputSystem.AddDevice<Keyboard>("Capybara title keyboard");
            device = keyboard;
            control = scenario == "e" || scenario == "connected-keyboard" ? keyboard.eKey : scenario == "space" ? keyboard.spaceKey : keyboard.enterKey;
            if (scenario == "connected-keyboard")
            {
                connectedPad = InputSystem.AddDevice<XInputController>("Connected Xbox during keyboard test");
                module.actionsAsset.devices = new[] { connectedPad };
            }
            if (scenario == "submenu") { menu.OpenSettingMenu(); expectStart = false; }
        }
        else if (scenario == "xinput")
        {
            var pad = InputSystem.AddDevice<XInputController>("Capybara title Xbox");
            device = pad;
            control = pad.buttonEast;
        }
        else if (scenario == "joystick")
        {
            var joystick = InputSystem.AddDevice<Joystick>("Capybara title joystick");
            device = joystick;
            control = joystick.trigger;
        }
        else if (scenario == "hid")
        {
            InputSystem.RegisterLayout(@"{""name"":""CapybaraTitleHID"",""extend"":""HID"",""format"":""CPHD"",""controls"":[{""name"":""button0"",""layout"":""Button"",""format"":""BIT"",""offset"":0,""bit"":0}]}",
                name: "CapybaraTitleHID", matches: new InputDeviceMatcher().WithInterface("HID").WithProduct("Capybara diagnostic HID"));
            device = InputSystem.AddDevice(new InputDeviceDescription { interfaceName = "HID", product = "Capybara diagnostic HID" });
            control = device.GetChildControl<ButtonControl>("button0");
        }
        else if (scenario == "mouse")
        {
            var mouse = InputSystem.AddDevice<Mouse>("Capybara title mouse");
            device = mouse;
            control = mouse.leftButton;
            expectStart = false;
        }
        else { Finish(false, "Unknown title test case: " + scenario); yield break; }

        try
        {
            // One frame lets the newly connected layout resolve its controls.
            yield return null;
            using (StateEvent.From(device, out var eventPtr))
            {
                control.WriteValueIntoEvent(1f, eventPtr);
                InputSystem.QueueEvent(eventPtr);
            }
            yield return new WaitForSecondsRealtime(0.4f);
            bool started = !menu.IsStartScreenActive && !menu.IsMenuUIOpen;
            Debug.Log($"CAPY_TITLE_CASE: {scenario}; layout={device.layout}; started={started}; expectStart={expectStart}; gameplayEnabled={actions.GamePlay.enabled}");
            Finish(started == expectStart && (!expectStart || actions.GamePlay.enabled), "title input case " + scenario);
        }
        finally
        {
            InputSystem.RemoveDevice(device);
            if (connectedPad != null) InputSystem.RemoveDevice(connectedPad);
        }
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
