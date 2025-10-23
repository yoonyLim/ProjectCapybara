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

            // --- ���� ---
            // y�� �ӵ��� ���� 0���� �����, �ϰ� �� �����ص� ������ ���̸� ���� (���� ����)
            player.rb.linearVelocity = new Vector3(player.rb.linearVelocity.x, 0, player.rb.linearVelocity.z);

            // ����� ������� �׻� �������� ���� ����
            player.rb.AddForce(Vector3.up * player.jumpForce, ForceMode.Impulse);
            // --- ���� �� ---

            //player.rb.useGravity = true;
        }
    }

    public void Exit(PlayerController player)
    {
        //player.LandSoundPlay();
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
            // �����ϸ� �޸��� ���·� ���� (���� �ִ� ó���� Running����)
            if (player.isGrounded && jumpApplied)
            {
                player.ChangeState(new RunningState());
                player.animator.SetBool("isFall", false);
                //player.animator.SetBool("isGrounded", true);
            }
            else
            {
                // ���߿��� �ϰ� ���̸� ���� �÷��׸�
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
        #region ���� �̵� �ڵ� (Running State - Move�� ���)
        // ī�޶� ���� ���� ���� ����
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
