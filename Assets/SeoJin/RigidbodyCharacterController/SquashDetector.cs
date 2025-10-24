using System;
using UnityEngine;

public class SquashDetector : MonoBehaviour
{
    private PlayerStateMachine stateMachine;

    private void Awake()
    {
        stateMachine = GetComponentInParent<PlayerStateMachine>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FallingRock") || other.CompareTag("BreakableRock")) 
        {
            stateMachine.Squash();
        }
    }
}
