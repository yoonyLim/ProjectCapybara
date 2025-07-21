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
    private DialogueNode currentNode;

    [SerializeField] 
    private TMP_Text speechBubbleText;

    [SerializeField]
    private DialogueState currentState;

    [SerializeField] 
    [Tooltip("Integer value for dialogue body scale effect. For example if set to 4, the character scales up and down every 4 characters of the dialogue.")]
    private int yScaleCharacterInterval = 4;
    [SerializeField]
    [Tooltip("Integer value for playing dialogue beep. For example if set to 2, the character beeps every 2 characters of the dialogue.")]
    private int dialogueBeepCharacterInterval = 2;
    [SerializeField] 
    private float timeBetweenCharacters = 0.05f;

    [SerializeField] 
    private float playerFacingSpeed = 200f;
    
    [SerializeField]
    private Transform meshTransform;

    private Vector3 originalScale;
    private Quaternion originalRotation;
    
    private AudioSource dialogueAudioSource;
    
    private Coroutine currentRotationCoroutine;
    
    private Coroutine currentDialogueCoroutine;

    private void Awake()
    {
        originalScale = meshTransform.localScale;
        originalRotation = meshTransform.localRotation;
        currentState = DialogueState.Inactive;
        speechBubbleText.transform.parent.gameObject.SetActive(false);
        dialogueAudioSource = GetComponent<AudioSource>();
    }

    public void StartDialogue()
    {
        currentNode = dialogueTree.RootNode;
        speechBubbleText.text = currentNode.DialogueText;
        speechBubbleText.transform.parent.gameObject.SetActive(true);
        currentDialogueCoroutine = StartCoroutine(ProcessDialogue());
        FacePlayer();
    }

    private void FacePlayer()
    {
        Vector3 directionToPlayer = GameObject.FindGameObjectWithTag("Player").transform.position - transform.position;
        directionToPlayer.y = 0;
        
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
        if (currentRotationCoroutine != null)
        {
            StopCoroutine(currentRotationCoroutine);
        }
        currentRotationCoroutine = StartCoroutine(RotateMesh(targetRotation));
    }

    private void FaceOriginalRotation()
    {
        if (currentRotationCoroutine != null)
        {
            StopCoroutine(currentRotationCoroutine);
        }
        currentRotationCoroutine = StartCoroutine(RotateMesh(originalRotation));
    }

    private IEnumerator RotateMesh(Quaternion targetRotation)
    {
        while (meshTransform.rotation != targetRotation)
        {
            meshTransform.rotation =
                Quaternion.RotateTowards(meshTransform.rotation, targetRotation, playerFacingSpeed * Time.deltaTime);
            
            yield return null;
        }
        
        meshTransform.rotation = targetRotation;
        currentRotationCoroutine = null;

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
                string dialogueText = currentNode.DialogueText;
                yield return StartCoroutine(TypeText(currentNode.DialogueText));
                currentNode = currentNode.NextNode;
            }
        }
        else
        {
            // Dialogue Ended.
            currentState = DialogueState.Inactive;
            speechBubbleText.transform.parent.gameObject.SetActive(false);
            FaceOriginalRotation();
            OnDialogueEnd?.Invoke();
        }

        currentDialogueCoroutine = null;

    }

    private IEnumerator TypeText(string dialogueText)
    {
        speechBubbleText.text = dialogueText;
        speechBubbleText.maxVisibleCharacters = 0;

        for (int i = 0; i < speechBubbleText.text.Length; i++)
        {
            if (i % yScaleCharacterInterval == 0)
            {
                StartCoroutine(ScaleBodyYTransform(1.1f, 4));
            }

            if (i % dialogueBeepCharacterInterval == 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, DialogueManager.Instance.MidtoneBeeps.Length);
                dialogueAudioSource.PlayOneShot(DialogueManager.Instance.MidtoneBeeps[randomIndex]);
            }
            
            speechBubbleText.maxVisibleCharacters++;
            yield return new WaitForSeconds(timeBetweenCharacters);
        }
    }

    private IEnumerator ScaleBodyYTransform(float yScaleCoefficient, int characterInterval)
    {
        float elapsedTime = 0;
        float halfDuration = timeBetweenCharacters * characterInterval / 2.0f;
        Vector3 targetScale = new Vector3(originalScale.x, originalScale.y * yScaleCoefficient, originalScale.z);
        
        while (elapsedTime < halfDuration)
        {
            meshTransform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / halfDuration);
            elapsedTime += Time.deltaTime;
            yield return null; 
        }
        
        elapsedTime = 0;
        
        while (elapsedTime < halfDuration)
        {
            meshTransform.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime / halfDuration);
            elapsedTime += Time.deltaTime;
            yield return null; 
        }
        
        meshTransform.localScale = originalScale;
    }
    

    private void OnChoiceSelected(DialogueChoice choice)
    {
        DialogueManager.Instance.DeleteChoiceButtons();
        currentNode = choice.NextNode;
        StartCoroutine(ProcessDialogue());
    }

    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && currentState == DialogueState.Normal)
        {
            if (currentDialogueCoroutine == null)
            {
                currentDialogueCoroutine = StartCoroutine(ProcessDialogue());
            }
            
        }
        
    }
}
