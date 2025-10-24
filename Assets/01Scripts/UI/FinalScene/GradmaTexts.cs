using UnityEngine;

public class GradmaTexts : MonoBehaviour
{
    private static readonly int Text1 = Animator.StringToHash("Text1");
    private static readonly int Text2 = Animator.StringToHash("Text2");
    private Animator anim;
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Text1In()
    {
        anim.SetTrigger(Text1);
    }

    public void Text2In()
    {
        anim.SetTrigger(Text2);
    }
}
