using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moko
{
    public class PlayerJump : MonoBehaviour, IPlayerModule
    {
        private PlayerInput _playerInput;
        private CharacterMotor motor;

        private float coyoteTimeCounter;
        private float jumpBufferCounter;

        private int jumpCount;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            motor = GetComponent<CharacterMotor>();
        }

        public Vector3 CalculateVelocity(CharacterMotor motor)
        {
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
                StartCoroutine(DisableGroundCheckForSeconds(0.2f));
                motor.playerAnimator.TriggerJumpInputParam();
                _playerInput.ClearJumpInput();
                jumpBufferCounter = motor.JumpData.JumpBufferTime;
                motor.stateMachine.audioSourceHolder.audioSources["Jump"].Play();
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

        private IEnumerator DisableGroundCheckForSeconds(float seconds)
        {
            motor.doGroundCheck = false;
            yield return new WaitForSeconds(seconds);
            motor.doGroundCheck = true;
        }

        //----------------------------------------------------------------
#if UNITY_EDITOR
        public float GetCoyoteTime() => coyoteTimeCounter;
        public float GetJumpBufferTime() => jumpBufferCounter;
#endif
    }
}