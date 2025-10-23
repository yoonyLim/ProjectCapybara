using UnityEngine.InputSystem;
using UnityEngine;

public class JumpState : IPlayerState
{
    private bool jumpApplied = false;
    private float smoothVel;
    private float entryTime;

    public void Enter(PlayerController player)
    {
        if(!jumpApplied)
        {
            entryTime = Time.time;
            Debug.Log("Jump Enter");
            player.isJumping = true;

            player.animator.SetInteger("Walk", 0);
            player.animator.SetTrigger("jumpTrigger");
            //player.isGrounded = false;

            // --- 수정 ---
            // y축 속도를 먼저 0으로 만들어, 하강 중 점프해도 일정한 높이를 보장 (선택 사항)
            player.rb.linearVelocity = new Vector3(player.rb.linearVelocity.x, 0, player.rb.linearVelocity.z);

            // 경사면과 상관없이 항상 위쪽으로 힘을 가함
            player.rb.AddForce(Vector3.up * player.jumpForce, ForceMode.Impulse);
            // --- 수정 끝 ---

            //player.rb.useGravity = true;
        }
    }

    public void Exit(PlayerController player)
    {
        player.animator.SetBool("isFall", false);
        Debug.Log("Jump Exit");
    }

    public void HandleInput(PlayerController player, InputAction.CallbackContext context) { }

    public void Update(PlayerController player)
    {
        player.isJumping = true;
        if (player.isGrounded && jumpApplied && Time.time > entryTime + 0.1f)
        {
            player.ChangeState(new RunningState());
            player.animator.SetBool("isFall", false);
        }
        else
        {
            // 착지하면 달리기 상태로 복귀 (착지 애니 처리도 Running에서)
            if (player.isGrounded && jumpApplied)
            {
                player.ChangeState(new RunningState());
                player.animator.SetBool("isFall", false);
                //player.animator.SetBool("isGrounded", true);
            }
            else
            {
                // 공중에서 하강 중이면 낙하 플래그만
                if (player.rb.linearVelocity.y < 0f)
                {

                    player.animator.SetBool("isFall", true);
                    jumpApplied = true;
                }
            }
        }
        
    }

    public void FixedUpdate(PlayerController player)
    {
        #region 공중 이동 코드 (Running State - Move와 비슷)
        // 카메라 기준 공중 제어 동일
        Vector3 camForward = player.cameraTransform.forward; camForward.y = 0; camForward.Normalize();
        Vector3 camRight = player.cameraTransform.right; camRight.y = 0; camRight.Normalize();

        Vector3 dir = camForward * player.MoveInput.y + camRight * player.MoveInput.x;
        player.moveDirection = dir.sqrMagnitude > 1f ? dir.normalized : dir;

        Vector3 desiredVel = dir * player.airSpeed;

        Vector3 v = player.rb.linearVelocity;
        Vector3 hv = new Vector3(v.x, 0f, v.z);
        Vector3 hvNew = Vector3.MoveTowards(hv, desiredVel, player.airAcceleration * Time.fixedDeltaTime);
        float newVerticalVelocity = player.rb.linearVelocity.y - player.gravity * Time.fixedDeltaTime;

        Quaternion RotationRef = player.SurfaceAlignment();

        Quaternion rotRef = player.SurfaceAlignment();

        if (player.moveDirection.magnitude > 0.01f)
        {
            float angle = Mathf.Atan2(player.moveDirection.x, player.moveDirection.z) * Mathf.Rad2Deg;
            float smooth = Mathf.SmoothDampAngle(player.transform.eulerAngles.y, angle, ref smoothVel, 0.1f);
            player.transform.rotation = Quaternion.Euler(rotRef.eulerAngles.x, smooth, rotRef.eulerAngles.z);
        }

        player.rb.linearVelocity = new Vector3(hvNew.x, newVerticalVelocity, hvNew.z);
        #endregion
    }
}
