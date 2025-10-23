using UnityEngine;
using UnityEngine.InputSystem;

public class SquashState : IPlayerState
{
    private float lockDrag = 99f;      // 미끄럼 방지(선택): 필요 없으면 주석 처리
    private float originalDrag;

    public void Enter(PlayerController player)
    {
        // 애니/속도 초기화
        if (player.animator) player.animator.SetInteger("Walk", 0);

        // 이동 완전 차단
        player.rb.linearVelocity = Vector3.zero;

        // 스쿼시 코루틴 시작 (끝날 때 RunningState로 복귀하도록 PlayerController에 이미 구현됨)
        player.StartSquashAndRecover();
    }

    public void Update(PlayerController player)
    {
        // 아무 것도 안 함: 입력 무시(상태 패턴으로 잠김)
    }

    public void FixedUpdate(PlayerController player)
    {
        // 물리적으로도 버벅임 없이 고정
        player.rb.linearVelocity = Vector3.zero;
        // 필요시 회전도 잠그고 싶다면:
        // player.rb.angularVelocity = Vector3.zero;
    }

    public void HandleInput(PlayerController player, InputAction.CallbackContext context)
    {
        // 입력 무시
    }

    public void Exit(PlayerController player)
    {
 
    }
}
