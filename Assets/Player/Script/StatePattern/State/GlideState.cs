using UnityEngine;
using UnityEngine.InputSystem;

public class GlideState : IPlayerState
{
    private float enterFallSpeed = -2f; // 진입 시 y속도 완화

    // 캐시
    private float targetGlideAngle;
    private float prevLinearDamping;
    private Vector3 prevGravity; // Physics.gravity 백업

    private float smoothVel;
    public void Enter(PlayerController player)
    {
        // 애니메이터 설정
        if (player.animator) player.animator.SetBool("isFly", true);

        // 물리 캐시 및 적용
        prevLinearDamping = player.rb.linearDamping;
        prevGravity = Physics.gravity;

        player.rb.linearDamping = player.glideDrag;
        Physics.gravity = new Vector3(0f, -player.glideGravity, 0f);

        // 낙하 초기 속도 완화
        var v = player.rb.linearVelocity;
        v.y = enterFallSpeed;
        player.rb.linearVelocity = v;

        // 시작 각도 목표
        if (player.moveDirection.sqrMagnitude > 0.01f)
        {
            targetGlideAngle = Mathf.Atan2(player.moveDirection.x, player.moveDirection.z) * Mathf.Rad2Deg;
        }
        else
        {
            targetGlideAngle = player.transform.eulerAngles.y;
        }
    }

    public void Exit(PlayerController player)
    {
        // 애니메이터 해제
        if (player.animator) player.animator.SetBool("isFly", false);

        // 물리 복구
        player.rb.linearDamping = prevLinearDamping;
        Physics.gravity = new Vector3(0f, -player.normalGravity, 0f);
    }

    public void HandleInput(PlayerController player, InputAction.CallbackContext context) { }

    public void Update(PlayerController player)
    {
        if (player.isGrounded)
        {
            player.ChangeState(new RunningState());
            return;
        }


        if (player.moveDirection.sqrMagnitude > 0.01f)
            targetGlideAngle = Mathf.Atan2(player.moveDirection.x, player.moveDirection.z) * Mathf.Rad2Deg;

        float newYaw = Mathf.MoveTowardsAngle(
            player.transform.eulerAngles.y,
            targetGlideAngle,
            player.glideTurnSpeed * Time.deltaTime
        );
        player.transform.rotation = Quaternion.Euler(0f, newYaw, 0f);
    }

    public void FixedUpdate(PlayerController player)
    {
        if (player.moveDirection.sqrMagnitude > 0.01f)
            targetGlideAngle = Mathf.Atan2(player.moveDirection.x, player.moveDirection.z) * Mathf.Rad2Deg;

        float newAngle = Mathf.MoveTowardsAngle(
            player.transform.eulerAngles.y,
            targetGlideAngle,
            player.glideTurnSpeed * Time.deltaTime
        );

        Quaternion rotRef = player.SurfaceAlignment();

        if (player.moveDirection.magnitude > 0.01f)
        {
            float angle = Mathf.Atan2(player.moveDirection.x, player.moveDirection.z) * Mathf.Rad2Deg;
            float smooth = Mathf.SmoothDampAngle(player.transform.eulerAngles.y, angle, ref smoothVel, 0.1f);
            player.transform.rotation = Quaternion.Euler(rotRef.eulerAngles.x, smooth, rotRef.eulerAngles.z);
        }
        //player.transform.rotation = Quaternion.Euler(0, newAngle, 0);

        Vector3 forward = player.transform.forward;
        player.rb.MovePosition(player.transform.position + forward * player.glideSpeed * Time.deltaTime);
    }

}

