using System;
using System.Collections;
using TMPro;
using UnityEngine;

enum DialogueState
{
    Inactive,
    Normal,
    Choice
}

[RequireComponent(typeof(AudioSource))]
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

    [SerializeField] 
    private float timeBetweenCharacters = 0.05f;
    
    [SerializeField]
    private Transform bodyTransform;
    
    private AudioSource dialogueAudioSource;

    private void Awake()
    {
        currentState = DialogueState.Inactive;
        speechBubbleText.transform.parent.gameObject.SetActive(false);
        dialogueAudioSource = GetComponent<AudioSource>();
    }

    public void StartDialogue()
    {
        currentNode = dialogueTree.RootNode;
        speechBubbleText.text = currentNode.DialogueText;
        speechBubbleText.transform.parent.gameObject.SetActive(true);
        StartCoroutine(ProcessDialogue());
    }

    public IEnumerator ProcessDialogue()
    {
        if (currentNode)
        {
            // Create choice buttons if there are choices. If not, proceed to the next dialogue node.
            if (currentNode.Choices.Count > 0)
            {
                currentState = DialogueState.Choice;
                yield return StartCoroutine(TypeText(currentNode.DialogueText));
                DialogueManager.Instance.CreateChoiceButtons(currentNode.Choices, OnChoiceSelected);
            }
            else
            {
                currentState = DialogueState.Normal;
                yield return StartCoroutine(TypeText(currentNode.DialogueText));
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

    private IEnumerator TypeText(string dialogueText)
    {
        speechBubbleText.text = dialogueText;
        speechBubbleText.maxVisibleCharacters = 0;

        for (int i = 0; i < speechBubbleText.text.Length; i++)
        {
            if (i % 4 == 0)
            {
                StartCoroutine(ScaleBodyYTransform(1.1f, 4));
            }

            if (i % 2 == 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, DialogueManager.Instance.MidtoneBeeps.Length);
                dialogueAudioSource.PlayOneShot(DialogueManager.Instance.MidtoneBeeps[randomIndex]);
            }
            
            speechBubbleText.maxVisibleCharacters++;
            yield return new WaitForSeconds(timeBetweenCharacters);
        }
    }

    private IEnumerator ScaleBodyYTransform(float maxYScale, int characterInterval)
    {
        float elapsedTime = 0;
        float halfDuration = timeBetweenCharacters * characterInterval / 2.0f;
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = new Vector3(originalScale.x, maxYScale, originalScale.z);
        
        while (elapsedTime < halfDuration)
        {
            bodyTransform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / halfDuration);
            elapsedTime += Time.deltaTime;
            yield return null; 
        }
        
        elapsedTime = 0;
        
        while (elapsedTime < halfDuration)
        {
            bodyTransform.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime / halfDuration);
            elapsedTime += Time.deltaTime;
            yield return null; 
        }
        
        bodyTransform.localScale = originalScale;
    }
    

    public void OnChoiceSelected(DialogueChoice choice)
    {
        DialogueManager.Instance.DeleteChoiceButtons();
        currentNode = choice.NextNode;
        StartCoroutine(ProcessDialogue());
    }

    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && currentState == DialogueState.Normal)
        {
            StartCoroutine(ProcessDialogue());
        }
        
    }
}
