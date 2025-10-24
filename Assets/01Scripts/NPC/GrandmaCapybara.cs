using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

public class GrandmaCapybara : MonoBehaviour
{
    [SerializeField] private Animator flashUIAnim;
    [SerializeField] private Animator endingCreditAnim;
    [SerializeField] PlayableDirector endingCreditTimeline;
    
    private static readonly int SadFace = Animator.StringToHash("SadFace");
    private static readonly int HappyFace = Animator.StringToHash("Idling");
    private static readonly int FlashIn = Animator.StringToHash("FlashIn");
    private static readonly int EndingCredit = Animator.StringToHash("EndingCredit");
    private Animator anim;
    
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    /*public void PlaySadFace()
    {
        anim.SetTrigger(SadFace);
    }*/

    public void PlayHappyFace()
    {
        anim.SetTrigger(HappyFace);
    }

    public void PlayFlashIn()
    {
        flashUIAnim.SetTrigger(FlashIn);
    }

    public void PlayEndingCredit()
    {
        endingCreditAnim.SetTrigger(EndingCredit);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            endingCreditTimeline.Play();
        }
    }
}
