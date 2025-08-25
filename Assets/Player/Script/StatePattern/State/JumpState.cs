using UnityEngine;
using UnityEngine.InputSystem;

public class JumpState : IPlayerState
{
    private Vector2 input;
    private float smoothVel;
    private bool jumpApplied = false;

    public void Enter(PlayerController player)
    {

        //player.animator.SetInteger("Walk", player.isRunning ? 2 : 1);
    }

    public void Exit(PlayerController player) { }

    public void HandleInput(PlayerController player, InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();

    }

    public void Update(PlayerController player)
    {
        // มกวม
    }

    public void FixedUpdate(PlayerController player)
    {

    }
}
