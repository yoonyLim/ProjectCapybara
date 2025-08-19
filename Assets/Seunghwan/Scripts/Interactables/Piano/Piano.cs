using UnityEngine;

public class Piano : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject brokenKeys;

    private bool isFixed = false;

    public void Interact()
    {
        if (PianoTracker.GetCanFixPiano() && !isFixed)
        {
            isFixed = true;
            brokenKeys.SetActive(false);
        }

        InteractionComponent.EndInteraction();
    }
}
