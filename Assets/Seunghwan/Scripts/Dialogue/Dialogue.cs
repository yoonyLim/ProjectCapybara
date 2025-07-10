using System;
using TMPro;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    [SerializeField]
    private DialogueTree dialogueTree;

    private DialogueNode currentNode;

    [SerializeField] 
    private TMP_Text speechBubbleText;

    public void StartDialogue()
    {
        speechBubbleText.transform.parent.gameObject.SetActive(true);
        currentNode = dialogueTree.RootNode;
        speechBubbleText.text = currentNode.DialogueText;
    }

    private void Awake()
    {
        speechBubbleText.transform.parent.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentNode)
        {
            if (currentNode.Choices.Count > 0)
            {
                
            }
            else
            {
                if (currentNode.NextNode)
                {
                    currentNode = currentNode.NextNode;
                    speechBubbleText.text = currentNode.DialogueText;
                }
                else
                {
                    currentNode = null;
                    speechBubbleText.transform.parent.gameObject.SetActive(false);
                }
            }
            
        }
    }
}
