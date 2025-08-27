using UnityEngine;
using UnityEngine.InputSystem;

public class GlideState : IPlayerState
{
    public void Enter(PlayerController player)
    {
        Debug.Log("GlideState Enter");
    }

    public void Exit(PlayerController player)
    {
        Debug.Log("GlideState Exit");
    }

    public void HandleInput(PlayerController player, InputAction.CallbackContext context) { }

    public void Update(PlayerController player)
    {

    }

    public void FixedUpdate(PlayerController player)
    {


    }
}
