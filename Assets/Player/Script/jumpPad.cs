using System;
using UnityEngine;

public class jumPad : MonoBehaviour
{
    public float strength;
    public Vector3 direction;
    private Animator jumpPadAnimator;
    private readonly int springAnimTrigger = Animator.StringToHash("Spring");
    
    private void Awake()
    {
        jumpPadAnimator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider player)
    {
        if (player.CompareTag("Player"))
        {
            Rigidbody rb = player.attachedRigidbody;
            var anim = player.GetComponent<Animator>();

            if (rb != null)
            {
                Vector3 vel = rb.linearVelocity;
                vel.y = 0;
                rb.linearVelocity = vel;
                
                rb.AddForce(direction.normalized * strength, ForceMode.Impulse);
            }
            
            jumpPadAnimator.SetTrigger(springAnimTrigger);
        }
    }
}
