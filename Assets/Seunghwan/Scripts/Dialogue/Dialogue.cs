using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

enum DialogueState
{
    Inactive,
    Normal,
    Choice
}

public class Dialogue : MonoBehaviour
{
    public static event Action OnDialogueEnd;
    [SerializeField]
    private DialogueTree dialogueTree;

    [SerializeField]
    private DialogueNode currentNode;

    [SerializeField] 
    private TMP_Text speechBubbleText;

    [SerializeField]
    private DialogueState currentState;
    

    public void StartDialogue()
    {
        currentNode = dialogueTree.RootNode;
        speechBubbleText.text = currentNode.DialogueText;
        speechBubbleText.transform.parent.gameObject.SetActive(true);
        ProcessDialogue();
    }

    public void ProcessDialogue()
    {
        if (currentNode)
        {
            // Create choice buttons if there are choices. If not, proceed to the next dialogue node.
            if (currentNode.Choices.Count > 0)
            {
                currentState = DialogueState.Choice;
                speechBubbleText.text = currentNode.DialogueText;
                DialogueManager.Instance.CreateChoiceButtons(currentNode.Choices, OnChoiceSelected);
            }
            else
            {
                currentState = DialogueState.Normal;
                speechBubbleText.text = currentNode.DialogueText;
                currentNode = currentNode.NextNode;
            }
        }
        else
        {
            // Dialogue Ended.
            currentState = DialogueState.Inactive;
            speechBubbleText.transform.parent.gameObject.SetActive(false);
            OnDialogueEnd?.Invoke();
        }
        
    }
    

    public void OnChoiceSelected(DialogueChoice choice)
    {
        DialogueManager.Instance.DeleteChoiceButtons();
        currentNode = choice.NextNode;
        ProcessDialogue();
    }

    private void Awake()
    {
        currentState = DialogueState.Inactive;
        speechBubbleText.transform.parent.gameObject.SetActive(false);
        
    }

    private void Update()
    {
        if (currentState == DialogueState.Normal)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                ProcessDialogue();
            }
        }
        
    }
}
