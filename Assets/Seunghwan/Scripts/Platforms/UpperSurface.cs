using System;
using UnityEngine;

public class UpperSurface : MonoBehaviour
{
    private TimedPlatform timedPlatform;

    private void Awake()
    {
        timedPlatform = GetComponentInParent<TimedPlatform>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timedPlatform.OnUpperSurfaceEnter(other);
        }
    }
}
