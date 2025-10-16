using System;
using UnityEngine;

public class PlayerFollowerTrigger : MonoBehaviour
{
    [SerializeField] private bool positiveTrigger;
    [SerializeField] private PlayerFollower targetFollower;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetFollower.followPlayer = positiveTrigger;
        }
    }
}
