using System;
using Moko;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private CharacterMotor motor;
    private PlayerInput playerInput;
    private Animator animator;
    
    private readonly int moveInputParam = Animator.StringToHash("moveInput");
    private readonly int sprintInputParam = Animator.StringToHash("sprintInput");
    private readonly int isGroundedParam  = Animator.StringToHash("isGrounded");
    private readonly int jumpInputParam  = Animator.StringToHash("jumpInput");
    private readonly int headbuttParam  = Animator.StringToHash("headbuttInput");

    private void Awake()
    {
        motor = GetComponent<CharacterMotor>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        UpdateMoveInputParam();
        UpdateSprintInputParam();
        UpdateIsGroundedParam();
    }

    private void UpdateMoveInputParam() => animator.SetBool(moveInputParam, playerInput.MoveInput != Vector2.zero);
    private void UpdateSprintInputParam() => animator.SetBool(sprintInputParam, playerInput.SprintInput);
    private void UpdateIsGroundedParam() => animator.SetBool(isGroundedParam, motor.IsOnValidGround);
    
    public void TriggerJumpInputParam() => animator.SetTrigger(jumpInputParam);
    public void TriggerHeadbuttInputParam() => animator.SetTrigger(headbuttParam);
}
