using UnityEngine;

public class InteractionComponent : MonoBehaviour
{

    [SerializeField] private LayerMask interactLayer;

    private IInteractable closestInteractable;

    private static bool IsInteracting = false;
    
    void Update()
    {
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

    public static void EndInteraction()
    {
        IsInteracting = false;
    }
}
