using UnityEngine;
using DualSenseUnity;

public class DualSenseFeedbackHandler : MonoBehaviour
{
    public void OnPlayerJumped()
    {
        //예시로 대충 아무거나 넣어봤음더
        DualSenseInputManager.Instance.RumbleControllerForDuration(0.2f, 0.1f);
    }

    public void OnPlayerLanded()
    {
        DualSenseInputManager.Instance.RumbleControllerForDuration(0.3f, 0.15f);
    }

    public void OnPlayerSwim()
    {
        DualSenseInputManager manager = DualSenseInputManager.Instance;
        manager.RumbleControllerWithCurve(manager.SwimRumbleCurve, manager.SwimRumbleDuration);
    }

}