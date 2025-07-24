using System;
using UnityEngine;

[RequireComponent(typeof(Dialogue))]
public class Animal : MonoBehaviour, IInteractable
{
    public enum FacialAnimationType
    {
        Eyes_Idle,
        Eyes_Annoyed,
        Eyes_Cry,
        Eyes_Excited,
        Eyes_Happy,
        Eyes_Shrink,
        Eyes_Spin,
        Eyes_Trauma
    }
    public enum AnimalState
    {
        Idle,
        Interacting
    }

    private Dialogue dialogue;
    private Animator animator;

    private int baseAnimationLayer;
    private int faceAnimationLayer;
    
    private AnimalState currentState = AnimalState.Idle;

    private void Awake()
    {
        dialogue = GetComponent<Dialogue>();
        animator = GetComponentInChildren<Animator>();
        baseAnimationLayer = animator.GetLayerIndex("Base Layer");
        faceAnimationLayer = animator.GetLayerIndex("Face Layer");
    }
    
    private void OnEnable()
    {
        dialogue.OnDialogueEnd += HandleDialogueEnd;
        dialogue.OnDialogueAdvance += HandleDialogueAdvance;
    }

    private void OnDisable()
    {
        dialogue.OnDialogueEnd -= HandleDialogueEnd;
        dialogue.OnDialogueAdvance -= HandleDialogueAdvance;
    }

    public void Interact()
    {
        dialogue.StartDialogue();
        ChangeState(AnimalState.Interacting);
    }

    public void ChangeState(AnimalState newState)
    {
        if (currentState == newState)
        {
            Debug.LogWarning("Animal.cs : Trying to change to a same state");
            return;
        }
        
        currentState = newState;
        TransitBaseAnimation(newState);
    }

    private void TransitBaseAnimation(AnimalState newState)
    {
        switch (newState)
        {
            case AnimalState.Idle:
            case AnimalState.Interacting:
                animator.CrossFadeInFixedTime("Idle", 0.3f, baseAnimationLayer);
                break;
            default:
                Debug.LogWarning("Animal.cs : Invalid animal state passed to TransitBaseAnimation function");
                break;
        }    
    }

    private void HandleDialogueEnd()
    {
        ChangeState(AnimalState.Idle);    
        ChangeFacialAnimation(Animal.FacialAnimationType.Eyes_Idle);
    }

    private void HandleDialogueAdvance()
    {
        ChangeFacialAnimation(dialogue.TargetFacialAnimation);
    }

    private void ChangeFacialAnimation(FacialAnimationType facialAnimation)
    {
        string targetStateName = facialAnimation.ToString();

        if (animator.GetCurrentAnimatorStateInfo(faceAnimationLayer).IsName(targetStateName)) return;
        
        animator.CrossFadeInFixedTime(targetStateName, 0.2f, faceAnimationLayer);

    }
    
    

    
}
