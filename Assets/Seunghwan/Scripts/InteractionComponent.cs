using System;
using UnityEngine;

public class InteractionComponent : MonoBehaviour
{

    [SerializeField] private LayerMask interactLayer;

    private IInteractable closestInteractable;
    
    public static bool IsInteracting = false;

    private Collider[] colliders = new Collider[10];

    private void Awake()
    {
        IsInteracting = false;
    }

    void Update()
    {
        closestInteractable = null;
        float minDistance = float.PositiveInfinity;

        if (!IsInteracting)
        {
            // Find the closest object which implements IInteractable interface.
            int colliderCount = Physics.OverlapSphereNonAlloc(transform.position, 5f, colliders, interactLayer);
            for (int i = 0; i < colliderCount; i++)
            {
                IInteractable interactable = colliders[i].GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    float distance = Vector3.Distance(transform.position, colliders[i].transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestInteractable = interactable;
                    }
                }
            }
            
            // If closest interactable object is found and interact key is pressed call interact function of the object.
            if (Input.GetKeyDown(KeyCode.E) && closestInteractable != null)
            {
                IsInteracting = true;
                closestInteractable.Interact();
            }
        }
        else
        {
            
        }
    }
    
    /// <summary>
    /// This function is used to end interaction to start querying for closest interactable objects again. It will be
    /// called inside classes such as Animal.cs, Piano.cs when the interaction is complete.
    /// </summary>
    public static void EndInteraction()
    {
        IsInteracting = false;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
#endif
}
