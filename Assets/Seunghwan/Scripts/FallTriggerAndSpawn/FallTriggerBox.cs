using System;
using UnityEngine;

public class FallTriggerBox : MonoBehaviour
{
    public static event Action OnPlayerFallTriggerEnter; 
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        OnPlayerFallTriggerEnter?.Invoke();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.0f, 1f, 0.05f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);

        Gizmos.color = new Color(1f, 0.0f, 1f, 1f);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
#endif
    
}
