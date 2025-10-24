using System;
using UnityEngine;

public class PenguinController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

    private Rigidbody rb;
    
    private bool shouldFlyWithCapy = false;
    private bool shouldFlyAway = false;
    
    public void FlyWithCapy()
    {
        shouldFlyWithCapy = true;
        rb.isKinematic = false;
    }

    public void FlyAway()
    {
        shouldFlyWithCapy = false;
        shouldFlyAway = true;
    }

    void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();
    }

    private void Update()
    {
        if (shouldFlyWithCapy)
        {
            transform.position = playerTransform.position + Vector3.up * 15;
        }

        if (shouldFlyAway)
        {
            rb.linearVelocity = Vector3.up * 500;
        }
    }
}
