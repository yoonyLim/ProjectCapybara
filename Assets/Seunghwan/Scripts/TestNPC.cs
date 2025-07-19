using System;
using UnityEngine;

[RequireComponent(typeof(Dialogue))]
public class TestNPC : MonoBehaviour, IInteractable
{
    private Dialogue dialogue;

    private void Awake()
    {
        dialogue = GetComponent<Dialogue>();
    }

    public void Interact()
    {
        Debug.Log("Interact Activated");
        dialogue.StartDialogue();
    }
}
