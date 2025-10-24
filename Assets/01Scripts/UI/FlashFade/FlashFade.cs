using UnityEngine;

public class FlashFade : MonoBehaviour
{
    private static readonly int FlashIn = Animator.StringToHash("FlashIn");
    private static readonly int FlashOut = Animator.StringToHash("FlashOut");
    private static readonly int FadeIn = Animator.StringToHash("FadeIn");
    private static readonly int FadeOut = Animator.StringToHash("FadeOut");
    private Animator anim;
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayFlashIn()
    {
        anim.SetTrigger(FlashIn);
    }

    public void PlayFlashOut()
    {
        anim.SetTrigger(FlashOut);
    }

    public void PlayFadeIn()
    {
        Debug.Log("fade in");
        anim.SetTrigger(FadeIn);
    }

    public void PlayFadeOut()
    {
        anim.SetTrigger(FadeOut);
    }
}
