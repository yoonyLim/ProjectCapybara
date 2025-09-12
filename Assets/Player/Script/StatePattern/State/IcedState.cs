using UnityEngine;
using UnityEngine.InputSystem;

public class IcedState : IPlayerState
{
    // 얼음에서 사용할 임펄스
    private const float iceImpulse = 0.4f;
    private float smoothVel;

    public void Enter(PlayerController player)
    {
        // 걷기 블렌드 영향 제거(선택)
        //player.animator.SetInteger("Walk", 0);
        // 얼음에서도 점프 가능하도록 중력 유지
        player.rb.useGravity = true;
        player.animator.SetBool("isIced", true);
        Debug.Log("iceState Enter");
    }

    public void Exit(PlayerController player)
    {
        Debug.Log("iceState Exit");
        player.animator.SetBool("isIced", false);
    }

    public void HandleInput(PlayerController player, InputAction.CallbackContext context) { }

    public void Update(PlayerController player)
    {

        // 얼음이 아니게 되면 러닝으로 복귀
        if (!player.isOnIce && player.isGrounded)
        {
            player.ChangeState(new RunningState());
        }
    }

    public void FixedUpdate(PlayerController player)
    {
        Vector3 camForward = player.cameraTransform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = player.cameraTransform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 dir = camForward * player.MoveInput.y + camRight * player.MoveInput.x;
        player.moveDirection = dir.sqrMagnitude > 1f ? dir.normalized : dir;

        Quaternion rotRef = player.SurfaceAlignment();

        if (player.moveDirection.magnitude > 0.01f)
        {
            float angle = Mathf.Atan2(player.moveDirection.x, player.moveDirection.z) * Mathf.Rad2Deg;
            float smooth = Mathf.SmoothDampAngle(player.transform.eulerAngles.y, angle, ref smoothVel, 0.5f); // 0.5f : smoothing 값 -> 커질수록 천천히 회전
            player.transform.rotation = Quaternion.Euler(rotRef.eulerAngles.x, smooth, rotRef.eulerAngles.z);
        }

        if (dir.sqrMagnitude > 0.0001f)
        {
            Vector3 velocity = dir.normalized;
            player.rb.AddForce(velocity * iceImpulse, ForceMode.Impulse);
        }

    }
}
