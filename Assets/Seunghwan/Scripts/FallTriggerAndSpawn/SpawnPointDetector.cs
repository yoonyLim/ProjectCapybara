using System;
using UnityEngine;

public class SpawnPointDetector : MonoBehaviour
{
    public static event Action<Vector3> OnSpawnPointTriggerEnter;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("On Trigger Enter");
        if (!other.CompareTag("SpawnPoint")) return;
        
        Debug.Log("Spawn Point Detected");
        
        Vector3 spawnPointPosition = other.GetComponentInParent<SpawnPoint>(true).GetSpawnPointPosition();
        OnSpawnPointTriggerEnter?.Invoke(spawnPointPosition);
    }
}
