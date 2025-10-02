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

    private float elapsedTime; // 유지 시간 추적s
    public void Enter(PlayerController player)
    {
        elapsedTime = 0f;

        // 애니메이터 설정
        if (player.animator) player.animator.SetBool("isFly", true);
        player.animator.SetBool("isGrounded", false);


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

        player.glideLocked = true;
    }

    public void HandleInput(PlayerController player, InputAction.CallbackContext context) { }

    public void Update(PlayerController player)
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= player.glideDuration)
        {
            player.ChangeState(new JumpState());
            return;
        }

        if (player.isGrounded)
        {
            player.ChangeState(new RunningState());
            player.animator.SetBool("isGrounded", true);
            return;
        }


        //if (player.moveDirection.sqrMagnitude > 0.01f)
        //    targetGlideAngle = Mathf.Atan2(player.moveDirection.x, player.moveDirection.z) * Mathf.Rad2Deg;

        //float newYaw = Mathf.MoveTowardsAngle(
        //    player.transform.eulerAngles.y,
        //    targetGlideAngle,
        //    player.glideTurnSpeed * Time.deltaTime
        //);
        //player.transform.rotation = Quaternion.Euler(0f, newYaw, 0f);
    }

    public void FixedUpdate(PlayerController player)
    {
        Vector2 input = player.MoveInput;
        Vector3 camF = player.cameraTransform.forward; camF.y = 0; camF.Normalize();
        Vector3 camR = player.cameraTransform.right; camR.y = 0; camR.Normalize();

        player.moveDirection = (camF * input.y + camR * input.x);
        if (player.moveDirection.sqrMagnitude > 0.0001f)
            player.moveDirection.Normalize();

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
            float smooth = Mathf.SmoothDampAngle(player.transform.eulerAngles.y, angle, ref smoothVel, 0.5f);
            player.transform.rotation = Quaternion.Euler(rotRef.eulerAngles.x, smooth, rotRef.eulerAngles.z);
        }

        Vector3 forward = player.transform.forward;
        player.rb.MovePosition(player.transform.position + forward * player.glideSpeed * Time.deltaTime);
    }

}

