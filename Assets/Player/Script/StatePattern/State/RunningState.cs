using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class RunningState : IPlayerState
{
    //private Vector2 input;
    private float smoothVel;

    public void Enter(PlayerController player)
    {
        
    }

    public void Exit(PlayerController player) { }

    public void HandleInput(PlayerController player, InputAction.CallbackContext context)
    {
        //input = context.ReadValue<Vector2>();
    }

    public void Update(PlayerController player)
    {
        // 점프, 글라이딩, 상호작용 추가
    }

    public void FixedUpdate(PlayerController player)
    {
        //얼음 위라면 IcedState로
        if (player.isOnIce)
        {
            player.ChangeState(new IcedState());
            return;
        }

        Move(player);
                  
    }

    public void Move(PlayerController player)
    {
        if (!player.isOnIce)
        {
            // 카메라를 기준으로 방향 구하기
            Vector3 camForward = player.cameraTransform.forward;
            camForward.y = 0;
            camForward.Normalize();

            Vector3 camRight = player.cameraTransform.right;
            camRight.y = 0;
            camRight.Normalize();

            Vector3 dir = camForward * player.MoveInput.y + camRight * player.MoveInput.x;
            player.moveDirection = dir.sqrMagnitude > 1f ? dir.normalized : dir;

            //------------------------------------

            float speed = player.isRunning ? 7f : 3.5f;

            // 캐릭터 회전 계산
            Quaternion rotRef = player.SurfaceAlignment();

            if (player.moveDirection.magnitude > 0.01f)
            {
                float angle = Mathf.Atan2(player.moveDirection.x, player.moveDirection.z) * Mathf.Rad2Deg;
                float smooth = Mathf.SmoothDampAngle(player.transform.eulerAngles.y, angle, ref smoothVel, 0.1f);
                player.transform.rotation = Quaternion.Euler(rotRef.eulerAngles.x, smooth, rotRef.eulerAngles.z);
            }

            Vector3 velocity = player.CalculateNextFrameGroundAngle(speed) < player.maxSlopeAngle ? player.moveDirection : Vector3.zero;
            Vector3 gravity = Vector3.down * Mathf.Abs(player.rb.linearVelocity.y);

            //------------------------------------

            // 경사로 위에 있을 때의 중력 및 속도 계산
            bool isOnSlope = player.IsOnSlope();
            if (isOnSlope && player.isGrounded)
            {
                velocity = player.AdjustDirectionToSlope(player.moveDirection);
                gravity = Vector3.zero;
                player.rb.useGravity = false;

                if (player.moveDirection.magnitude < 0.01f)
                {
                    player.rb.linearVelocity = Vector3.zero;
                }
                else
                {
                    player.rb.linearVelocity = velocity * speed + gravity;
                }
            }
            else
            {
                player.rb.useGravity = true;
                Vector3 currentVelocity = player.rb.linearVelocity;
                Vector3 targetVelocity = new Vector3(velocity.x * speed, currentVelocity.y, velocity.z * speed);
                player.rb.linearVelocity = targetVelocity;
            }

            // 애니메이션
            bool isMoving = player.moveDirection.sqrMagnitude > 0.001f;
            if (player.isGrounded)
            {
                player.animator.SetInteger("Walk", isMoving ? (player.isRunning ? 2 : 1) : 0);
            }
        }
        
    }

}
