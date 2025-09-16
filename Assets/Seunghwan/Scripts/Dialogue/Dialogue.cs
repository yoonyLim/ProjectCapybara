using System;
using System.Collections;
using TMPro;
using UnityEngine;


[RequireComponent(typeof(Animal))]
public class Dialogue : MonoBehaviour
{
    [SerializeField] 
    private AudioClip[] speechBeepSounds;
    enum DialogueState
    {
        Inactive,
        Normal,
        // Choice
    }
    public event Action OnDialogueStart;
    public event Action OnDialogueEnd;
    public event Action OnDialogueAdvance;
    
    
    [SerializeField]
    private DialogueTree dialogueTree;
    private DialogueNode currentNode;
    
    private TMP_Text speechBubbleText;

    [SerializeField]
    private DialogueState currentState;
    
    public Animal.FacialAnimationType TargetFacialAnimation;

    // [SerializeField] 
    // [Tooltip("Integer value for dialogue body scale effect. For example if set to 4, the character scales up and down every 4 characters of the dialogue.")]
    // private int yScaleCharacterInterval = 4;
    
    [SerializeField]
    [Tooltip("Integer value for playing dialogue beep. For example if set to 2, the character beeps every 2 characters of the dialogue.")]
    private int dialogueBeepCharacterInterval = 2;
    [SerializeField] 
    private float timeBetweenCharacters = 0.05f;
    
    private float rotateSpeed = 150f;

    // private Vector3 originalScale;
    private Quaternion originalRotation;
    
    private AudioSource dialogueAudioSource;
    
    private Coroutine currentRotationCoroutine;
    
    private Coroutine currentDialogueCoroutine;

    private void Awake()
    {
        speechBubbleText = GetComponentInChildren<TMP_Text>();
        // originalScale = meshTransform.localScale;
        originalRotation = transform.rotation;
        currentState = DialogueState.Inactive;
        speechBubbleText.transform.parent.gameObject.SetActive(false);
        dialogueAudioSource = GetComponent<AudioSource>();
    }

    public void StartDialogue()
    {
        OnDialogueStart?.Invoke();
        
        currentNode = dialogueTree.RootNode;
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
        while (transform.rotation != targetRotation)
        {
            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
            
            yield return null;
        }

        transform.rotation = targetRotation;
        currentRotationCoroutine = null;

    }

    public IEnumerator ProcessDialogue()
    {
        
        if (currentNode)
        {
            TargetFacialAnimation = currentNode.FacialAnimation;
            OnDialogueAdvance?.Invoke();
            currentState = DialogueState.Normal;
            string dialogueText = currentNode.DialogueText;
            yield return StartCoroutine(TypeText(currentNode.DialogueText));
            currentNode = currentNode.NextNode;
            
            // Create choice buttons if there are choices. If not, proceed to the next dialogue node.
            // if (currentNode.Choices.Count > 0)
            // {
            //     currentState = DialogueState.Choice;
            //     yield return StartCoroutine(TypeText(currentNode.DialogueText));
            //     DialogueManager.Instance.CreateChoiceButtons(currentNode.Choices, OnChoiceSelected);
            // }
            // else
            // {
            //     currentState = DialogueState.Normal;
            //     string dialogueText = currentNode.DialogueText;
            //     yield return StartCoroutine(TypeText(currentNode.DialogueText));
            //     currentNode = currentNode.NextNode;
            // }
        }
        else
        {
            // Dialogue Ended.
            
            currentState = DialogueState.Inactive;
            speechBubbleText.transform.parent.gameObject.SetActive(false);
            FaceOriginalRotation();
            StartCoroutine(EndInteractionCoroutine()); // TODO: This is a temporary function. Should create event-based manger later on.
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
            // if (i % yScaleCharacterInterval == 0)
            // {
            //     StartCoroutine(ScaleBodyYTransform(1.1f, 4));
            // }

            if (i % dialogueBeepCharacterInterval == 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, speechBeepSounds.Length);
                // TODO: Change playing audio logic to using audio manager
                dialogueAudioSource.PlayOneShot(speechBeepSounds[randomIndex]);
            }
            
            speechBubbleText.maxVisibleCharacters++;
            yield return new WaitForSeconds(timeBetweenCharacters);
        }
    }

    // private IEnumerator ScaleBodyYTransform(float yScaleCoefficient, int characterInterval)
    // {
    //     float elapsedTime = 0;
    //     float halfDuration = timeBetweenCharacters * characterInterval / 2.0f;
    //     Vector3 targetScale = new Vector3(originalScale.x, originalScale.y * yScaleCoefficient, originalScale.z);
    //     
    //     while (elapsedTime < halfDuration)
    //     {
    //         meshTransform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / halfDuration);
    //         elapsedTime += Time.deltaTime;
    //         yield return null; 
    //     }
    //     
    //     elapsedTime = 0;
    //     
    //     while (elapsedTime < halfDuration)
    //     {
    //         meshTransform.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime / halfDuration);
    //         elapsedTime += Time.deltaTime;
    //         yield return null; 
    //     }
    //     
    //     meshTransform.localScale = originalScale;
    // }
    

    // private void OnChoiceSelected(DialogueChoice choice)
    // {
    //     DialogueManager.Instance.DeleteChoiceButtons();
    //     currentNode = choice.NextNode;
    //     StartCoroutine(ProcessDialogue());
    // }

    /// <summary>
    /// Made this function to delay setting IsInteracting to false by 1 frame to avoid instant interaction restart when
    /// pressing the interaction key (Currently set to E).
    /// </summary>
    /// <returns></returns>
    private IEnumerator EndInteractionCoroutine()
    {
        yield return new WaitForEndOfFrame();
        InteractionComponent.EndInteraction();
        
    }

    private void Update()
    {
        // TODO: Currently hard coded to keycode values. Should be refactored with New Input System.
        if ((Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.E)) && currentState == DialogueState.Normal)
        {
            if (currentDialogueCoroutine == null)
            {
                currentDialogueCoroutine = StartCoroutine(ProcessDialogue());
            }
            
        }
        
    }
}
