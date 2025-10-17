// HitState.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class HitState : IPlayerState
{
    private float hitDuration = 1f; // 피격 상태 지속 시간 (애니메이션 길이에 맞게 조절)
    private float timer;

    public void Enter(PlayerController player)
    {
        Debug.Log("HitState Enter");
        timer = 0f;

        player.animator.SetInteger("Walk", 0);
        player.animator.Play("Spin");
    }

    public void Update(PlayerController player)
    {
        timer += Time.deltaTime;

        if (timer >= hitDuration)
        {
            player.ChangeState(new RunningState());
        }
    }

    public void FixedUpdate(PlayerController player)
    {

    }

    public void HandleInput(PlayerController player, InputAction.CallbackContext context)
    {

    }

    public void Exit(PlayerController player)
    {
        Debug.Log("HitState Exit");
    }
}