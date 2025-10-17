using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DualSenseUnity;
using UnityEngine.UIElements;

public class DualSenseInputManager : MonoBehaviour
{
    public static DualSenseInputManager Instance { get; private set; }
    
    [Header("Rumble Pattern")]
    public AnimationCurve SwimRumbleCurve;
    public float SwimRumbleDuration = 2f;
    
    [HideInInspector]
    public uint ControllerCount = 0;
    public List<DualSenseController> DualSenseControllers = new List<DualSenseController>();
    
    private ControllerOutputState _outputState = new ControllerOutputState();
    
    private Coroutine dualSenseRumbleCoroutine;
    
    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    private void RefreshControllers()
    {
        ControllerCount = DualSense.GetControllerCount();
        DualSenseControllers = DualSense.GetControllers();
    }

    private void Start()
    {
        DualSense.ControllerCountChanged += RefreshControllers;
        RefreshControllers();
    }
    
    /**
     * [DualSense inputs]
     * 
     * DPad Buttons(bool): inputState.DPad[Up|Down|Left|Right]Button.[Up|NewUp|Down|NewDown]
     * Action Buttons(bool): inputState.[Circle|Cross|Square|Triangle]Button.[Up|NewUp|Down|NewDown]
     * Stick Buttons(bool): inputState.[Left|Right]Stick.PushButton
     * Stick Tilt(double)[-1.0, 1.0]: inputState.[Left|Right]Stick.[X|Y]Axis
     * R1|L1 Buttons(bool): inputState.[Left|Right]Bumper.[Up|NewUp|Down|NewDown]
     * R2|L2 Buttons(bool): inputState.[Lef|Right]Trigger.ActiveState.[Up|NewUp|Down|NewDown]
     * R2|L2 Buttons Trigger(double)[0.0, 1.0]: inputState.[Lef|Right]Trigger.TriggerValue
     * Create Button(bool): inputState.CreateButton.[Up|NewUp|Down|NewDown]
     * Options Button(bool): inputState.OptionsButton.[Up|NewUp|Down|NewDown]
     * PS Button(bool): inputState.PSButton.[Up|NewUp|Down|NewDown]
     * Microphone Button(bool): inputState.MicrophoneButton.[Up|NewUp|Down|NewDown]
     * TouchPad Points(double)[0.0, 1.0]: inputState.TouchPad.TouchPoint[1|2].[X|Y] -> X [0.0, 1.0] means [Left, Right], Y [0.0, 1.0] means [Up, Down]
     * Accelerometer(double)[0.0, 1.0]: inputState.Accelerometer.[X|Y|Z]Axis
     * Gyroscope(double)[0.0, 1.0]: inputState.Gyroscope.[X|Y|Z]Axis
     * HeadPhone Connection(bool): inputState.HeadPhonesConnected
     *
     * [DualSense Outputs]
     *
     * Rumble: _outputState.[Right|Left]RumbleIntensity = [0.0, 1.0]
     * 
     */
    
    private bool IsButtonPressed( ButtonState buttonState )
    {
        return buttonState == ButtonState.NewDown;
    }

    private bool IsButtonBeingPressed(ButtonState buttonState)
    {
        return buttonState == ButtonState.Down || buttonState == ButtonState.NewDown;
    }

    private bool IsButtonReleased(ButtonState buttonState)
    {
        return buttonState == ButtonState.NewUp;
    }

    public void RumbleController(float intensity)
    {
        _outputState.LeftRumbleIntensity = intensity;
        _outputState.RightRumbleIntensity = intensity;
    }

    public void RumbleControllerWithCurve(AnimationCurve curve, float duration)
    {
        if (dualSenseRumbleCoroutine != null)
            StopCoroutine(dualSenseRumbleCoroutine);

        dualSenseRumbleCoroutine = StartCoroutine(RumblePatternWithCurve(curve, duration));
    }
    
    IEnumerator RumblePatternWithCurve(AnimationCurve curve, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            
            _outputState.LeftRumbleIntensity = curve.Evaluate(timer / duration);
            _outputState.RightRumbleIntensity = curve.Evaluate(timer / duration);
            
            yield return null;
        }

        _outputState.LeftRumbleIntensity = 0f;
        _outputState.RightRumbleIntensity = 0f;
    }

    public void RumbleControllerForDuration(float intensity, float duration)
    {
        if (dualSenseRumbleCoroutine != null)
            StopCoroutine(dualSenseRumbleCoroutine);
        
        dualSenseRumbleCoroutine = StartCoroutine(RumblePattern(intensity, duration));
    }
    
    IEnumerator RumblePattern(float intensity, float duration)
    {
        _outputState.LeftRumbleIntensity = intensity;
        _outputState.RightRumbleIntensity = intensity;  
        yield return new WaitForSeconds(duration);
        _outputState.LeftRumbleIntensity = 0f;
        _outputState.RightRumbleIntensity = 0f;
    }

    private void Update()
    {
        if (DualSenseControllers.Count == 0)
            return;
        
        var controllerToUse = DualSenseControllers[0];
        var inputState = controllerToUse.GetInputState();
        
        // interact with DualSense logic
        if (IsButtonPressed( inputState.DPadUpButton ))
            Debug.Log("DPad Up pressed");
        
        if (IsButtonPressed( inputState.CreateButton ))
            Debug.Log("Create Btn pressed");
        
        if (IsButtonPressed( inputState.LeftTrigger.ActiveState ))
            RumbleControllerForDuration(0.2f, 1f);
        
        if (IsButtonPressed( inputState.RightTrigger.ActiveState ))
            RumbleControllerWithCurve(SwimRumbleCurve, SwimRumbleDuration);
        
        controllerToUse.SetOutputState( _outputState );
    }
}
