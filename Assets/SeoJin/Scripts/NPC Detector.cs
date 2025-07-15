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
                    Debug.Log("Detect NPC: " + collider.gameObject.name);
                    
                    float squaredDistance = (collider.transform.position - transform.position).sqrMagnitude;

                    if (squaredDistance < minDistanceSquared)
                    {
                        minDistanceSquared = squaredDistance;
                        closestTarget = collider.gameObject;
                    }
                }
                // allocate closestTarget to currentDetectedNPC
                CurrentDetectedNPC = closestTarget;
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
