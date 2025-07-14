using System;
using System.Collections;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using Random = UnityEngine.Random;

public class NPCDetector : MonoBehaviour
{
    [SerializeField] private float detectRadius;
    [SerializeField] private LayerMask NPCLayerMask;
    [SerializeField] private float checkInterval = 0.1f;

    private GameObject closestTarget;
    
    
    // Exposed by Getter, Get a CurrentDetectedNPC!!!! 
    public GameObject currentDetectedNPC { get; private set; }

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
                // Find Closest Target
                foreach (Collider collider in colliders)
                {
                    float minDistanceSquared = detectRadius;
                    float squaredDistance = (collider.transform.position - transform.position).sqrMagnitude;

                    if (squaredDistance < minDistanceSquared)
                    {
                        minDistanceSquared = squaredDistance;
                        closestTarget = collider.gameObject;
                    }
                }
                currentDetectedNPC = closestTarget; // allocate closestTarget to currentDetectedNPC
            }
            else
            {
                currentDetectedNPC = null; // make sure if nothing detected
            }
            
            yield return new WaitForSeconds(checkInterval);
        }
    }
    
    
    
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = currentDetectedNPC ? Color.red : Color.blue;
        
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
    
    
    #endif
}
