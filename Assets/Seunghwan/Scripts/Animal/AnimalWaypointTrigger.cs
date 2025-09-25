using System;
using UnityEngine;

public class AnimalWaypointTrigger : MonoBehaviour
{
    public event Action<AnimalWaypointTrigger> OnPlayerTriggerEnter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerTriggerEnter?.Invoke(this);
        }
    }
}