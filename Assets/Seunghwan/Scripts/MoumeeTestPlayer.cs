using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MoumeeTestPlayer : MonoBehaviour
{
    private Collider interactCollider;
    private CharacterController characterController;
    private float moveSpeed = 10.0f;
    
    [SerializeField] 
    private LayerMask interactLayer;
    
    private IInteractable closestInteractable;

    public bool IsInteracting = false;
    

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        
        
    }

    private void OnEnable()
    {
        Dialogue.OnDialogueEnd += EndInteraction;
    }

    private void OnDisable()
    {
        Dialogue.OnDialogueEnd -= EndInteraction;
    }
    

    private void Update()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        characterController.SimpleMove(input *  moveSpeed);
        
        closestInteractable = null;
        float minDistance = float.PositiveInfinity;

        if (!IsInteracting)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 20.0f, interactLayer);
            foreach (Collider collider in colliders)
            {
                IInteractable interactable = collider.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < minDistance)
                    {
                        closestInteractable = interactable;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.E) && closestInteractable != null)
            {
                IsInteracting = true;
                closestInteractable.Interact();
            }
        }
    }

    private void EndInteraction()
    {
        IsInteracting = false;
    }

   

    
    
    
}
