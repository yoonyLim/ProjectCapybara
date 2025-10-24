using System;
using UnityEngine;

public class RockBreaker : MonoBehaviour
{
    private Collider collider;

    private void Awake()
    {
        collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BreakableRock"))
        {
            other.TryGetComponent<IDestructible>(out IDestructible destructible);
            destructible.Hit();
        }
    }
}
