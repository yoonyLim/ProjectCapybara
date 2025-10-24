using UnityEngine;

namespace Moko
{
    public class PlayerJump : MonoBehaviour, IPlayerModule
    {
        private PlayerInput _playerInput;

        private float coyoteTimeCounter;
        private float jumpBufferCounter;

        private int jumpCount;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
        }

        public Vector3 CalculateVelocity(CharacterMotor motor)
        {
            Debug.Log(jumpCount);
            
            if (motor.IsOnValidGround)
            {
                coyoteTimeCounter = motor.JumpData.CoyoteTime;
                jumpCount = motor.JumpData.maxJumps - 1;
            }
            else
            {
                coyoteTimeCounter -= Time.fixedDeltaTime;
            }


            bool jumpPressed = _playerInput.JumpInput;
            if (jumpPressed)
            {
                motor.playerAnimator.TriggerJumpInputParam();
                _playerInput.ClearJumpInput();
                jumpBufferCounter = motor.JumpData.JumpBufferTime;
            }
            else
            {
                jumpBufferCounter -= Time.fixedDeltaTime;
            }

            if (jumpBufferCounter > 0f)
            {
                if (coyoteTimeCounter > 0f)
                {
                    coyoteTimeCounter = 0f;
                    
                    jumpBufferCounter = 0f;
                    motor.PlayerGravity.SetVerticalVelocity(motor.JumpData.JumpForce);
                }
                else if (jumpCount > 0)
                {
                    jumpCount--; 
                    
                    jumpBufferCounter = 0f;
                    motor.PlayerGravity.SetVerticalVelocity(motor.JumpData.JumpForce);
                }
            }
            
            return Vector3.zero;
        }

        //----------------------------------------------------------------
#if UNITY_EDITOR
        public float GetCoyoteTime() => coyoteTimeCounter;
        public float GetJumpBufferTime() => jumpBufferCounter;
#endif
    }
}