using UnityEngine;

public class FilmFrame : MonoBehaviour
{
    private static readonly int SIn = Animator.StringToHash("SlideIn");
    private static readonly int SOut = Animator.StringToHash("SlideOut");
    private Animator anim;
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void SlideIn()
    {
        anim.SetTrigger(SIn);
    }

    public void SlideOut()
    {
        anim.SetTrigger(SOut);
    }
}
