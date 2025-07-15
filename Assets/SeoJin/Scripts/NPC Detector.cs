using System.Collections;
using Moko;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class NPCDetector : MonoBehaviour
{
    [SerializeField] private float detectRadius;
    [SerializeField] private LayerMask NPCLayerMask;
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private float checkInterval = 0.1f;

    
    // Exposed by Getter, Get a CurrentDetectedNPC!!!! 
    public GameObject CurrentDetectedNPC { get; private set; }
    private GameObject closestTarget;
    
    private void Start()
    {
        StartCoroutine(DetectNPC());
    }

    // NPC Detection via OverlapSphere
    private IEnumerator DetectNPC()
    {
        yield return new WaitForSeconds(Random.Range(0, 0.5f));

        while (true)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectRadius, NPCLayerMask);

            if (colliders.Length > 0)
            {
                float minDistanceSquared = detectRadius * detectRadius;
                closestTarget = null;
                
                // Find Closest Target
                foreach (Collider collider in colliders)
                {
                    
                    float squaredDistance = (collider.transform.position - transform.position).sqrMagnitude;

                    if (squaredDistance < minDistanceSquared)
                    {
                        minDistanceSquared = squaredDistance;
                        closestTarget = collider.gameObject;
                    }
                }
                // allocate closestTarget to currentDetectedNPC

                if (closestTarget)
                {
                    Vector3 rayOrigin = transform.position + (Vector3.up * 0.3f);
                    Vector3 targetPosition = closestTarget.transform.position + (Vector3.up * 0.3f); // 타겟도 약간 위로
                    Vector3 rayDirection = (targetPosition - rayOrigin).normalized;
                    float rayDistance = Vector3.Distance(rayOrigin, targetPosition);
                    
                    bool isHit = Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, rayDistance, obstacleLayerMask);
                    if (!isHit || hit.collider.gameObject == closestTarget)
                    {
                        CurrentDetectedNPC = closestTarget;
                        DebugExtension.ColorLog("Detect NPC: " + closestTarget.name, "green");
                    }
                    else
                    {
                        DebugExtension.ColorLog($"Line of sight blocked by: {hit.collider.gameObject.name}", "red");
                    }
                }
            }
            else
            {
                ClearCurrentDetectedNPC(); // make sure if nothing detected
            }
            
            yield return new WaitForSeconds(checkInterval);
        }
    }

    public void ClearCurrentDetectedNPC()
    {
        CurrentDetectedNPC = null;
    }
    
    
    
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = CurrentDetectedNPC ? Color.red : Color.blue;
        
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
    #endif
}
