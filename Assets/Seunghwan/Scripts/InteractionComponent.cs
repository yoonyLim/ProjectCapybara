using Capybara;
using System;
using UnityEngine;

/// <summary>
/// Handles player interaction with the environment.
/// This component scans for nearby IInteractable objects and triggers their
/// Interact() method when the player presses the interact button (via InputReader).
/// </summary>
public class InteractionComponent : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Assign the CapybaraInputReader asset here.")]
    [SerializeField] private CapybaraInputReader inputReader;

    [Header("Interaction Settings")]
    [Tooltip("The layer(s) that contain interactable objects.")]
    [SerializeField] private LayerMask interactLayer;
    [Tooltip("The radius to scan for interactable objects.")]
    [SerializeField] private float interactRadius = 5f;

    private IInteractable closestInteractable;
    private static bool IsInteracting = false;
    private Collider[] colliders = new Collider[10];

    public static event Action OnDialogEnd;
    public static event Action OnDialogStart;
    private void Awake()
    {
        IsInteracting = false;
        closestInteractable = null;
    }

    /// <summary>
    /// Subscribes to the InteractEvent from the InputReader.
    /// </summary>
    private void OnEnable()
    {
        if (inputReader == null)
        {
            Debug.LogError("InputReader is not assigned in InteractionComponent!", this);
            return;
        }

        inputReader.InteractEvent += HandleInteractInput;
    }

    /// <summary>
    /// Unsubscribes from the InteractEvent.
    /// </summary>
    private void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.InteractEvent -= HandleInteractInput;
        }
    }

    private void Update()
    {
        // Only search for interactables if we are not currently in an interaction.
        if (!IsInteracting)
        {
            FindClosestInteractable();
        }
        else
        {
            // Clear the target once an interaction has begun.
            closestInteractable = null;
        }
    }

    /// <summary>
    /// Finds the closest object implementing IInteractable and stores it.
    /// </summary>
    private void FindClosestInteractable()
    {
        closestInteractable = null;
        float minDistance = float.PositiveInfinity;

        int colliderCount = Physics.OverlapSphereNonAlloc(transform.position, interactRadius, colliders, interactLayer);

        for (int i = 0; i < colliderCount; i++)
        {
            // Find the component on the parent, allowing colliders on child objects.
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
    }

    /// <summary>
    /// Handles the Interact input event received from the InputReader.
    /// </summary>
    private void HandleInteractInput()
    {
        // 1. UIManager 상태 확인
        if (UIManager.instance != null && (UIManager.instance.IsMenuUIOpen || UIManager.instance.IsDialogueActive))
        {
            return;
        }

        // 2. 이미 상호작용 중인지 확인
        if (IsInteracting)
        {
            return;
        }

        // 3. 상호작용할 대상이 있는지 확인
        if (closestInteractable != null)
        {
            IsInteracting = true;
            closestInteractable.Interact();
            OnDialogStart?.Invoke();
        }
    }

    /// <summary>
    /// Public static method to be called by other scripts (e.g., Dialogue) 
    /// when an interaction is finished, allowing the player to interact again.
    /// </summary>
    public static void EndInteraction()
    {
        IsInteracting = false;
        OnDialogEnd?.Invoke();
    }

#if UNITY_EDITOR
    /// <summary>
    /// Draws the interaction radius in the editor for easy visualization.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
#endif
}