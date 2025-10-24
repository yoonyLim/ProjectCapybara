using System;
using Moko;
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
            PlayerGravity playerGravity = player.GetComponentInParent<PlayerGravity>();
            PlayerJump playerJump = player.GetComponentInParent<PlayerJump>();

            playerJump.DisableGroundCheck(0.2f);
            playerGravity.SetVerticalVelocity(strength);
            
            DualSenseInputManager.Instance.RumbleControllerShort(1);

            jumpPadAnimator.SetTrigger(springAnimTrigger);
        }
    }
}
