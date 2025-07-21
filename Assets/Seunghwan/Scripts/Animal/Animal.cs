using UnityEngine;

[RequireComponent(typeof(Dialogue))]
public class Animal : MonoBehaviour, IInteractable
{
    private Dialogue dialogue;

    private void Awake()
    {
        dialogue = GetComponent<Dialogue>();
    }

    public void Interact()
    {
        dialogue.StartDialogue();
    }
}
