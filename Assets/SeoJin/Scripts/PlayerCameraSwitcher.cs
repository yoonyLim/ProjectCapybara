using UnityEngine;


/// <summary>
/// Class that Exposes Camera Switch Methods
/// </summary>
public class PlayerCameraSwitcher : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SwitchToDialogue()
    {
        animator.Play("PlayerDialogueCamera");
    }

    public void SwitchToTopView()
    {
        animator.Play("PlayerCamera");
    }





    // -----------------------test code-----------------------
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerCamera"))
            {
                SwitchToDialogue();
            }
            else
            {
                SwitchToTopView();
            }
        }
    }
    // -----------------------test code-----------------------
}
