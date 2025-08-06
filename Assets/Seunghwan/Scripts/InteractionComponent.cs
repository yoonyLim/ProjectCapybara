using System;
using UnityEngine;

public class InteractionComponent : MonoBehaviour
{

    [SerializeField] private LayerMask interactLayer;

    private IInteractable closestInteractable;
    
    private static bool IsInteracting = false;

    private Collider[] colliders = new Collider[5];

    private void Awake()
    {
        IsInteracting = false;
    }

    void Update()
    {
        Debug.Log(IsInteracting);
        
        closestInteractable = null;
        float minDistance = float.PositiveInfinity;

        if (!IsInteracting)
        {
            int colliderCount = Physics.OverlapSphereNonAlloc(transform.position, 5f, colliders, interactLayer);
            for (int i = 0; i < colliderCount; i++)
            {
                IInteractable interactable = colliders[i].GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    float distance = Vector3.Distance(transform.position, colliders[i].transform.position);
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
