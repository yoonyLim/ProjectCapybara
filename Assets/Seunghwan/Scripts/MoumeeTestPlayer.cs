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
    

    private void Update()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        characterController.SimpleMove(input *  moveSpeed);
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, 20.0f, interactLayer);
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out IInteractable interactable))
            {
                closestInteractable = interactable;
                break;
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && closestInteractable != null && !IsInteracting)
        {
            IsInteracting = true;
            closestInteractable.Interact();
        }
        
    }

   

    
    
    
}
