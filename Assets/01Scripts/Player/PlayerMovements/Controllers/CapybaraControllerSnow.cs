using System;
using System.Runtime.CompilerServices;
using System.Collections;
using UnityEngine;

namespace Capybara
{
    public class CapybaraControllerSnow : MonoBehaviour
    {
        [SerializeField] private CapybaraInputReader inputReader;
        
        [Header("Movement Settings")]
        [SerializeField] private float constMoveSpeed = 3.5f;
        [SerializeField] private float constSprintSpeed = 7f;
        [SerializeField] private float constJumpSpeed = 6f;
        
        [Header("Glide Settings")]
        [SerializeField] private float glideSpeed = 4.5f;
        [SerializeField] private float glideGravity = 4.0f;
        [SerializeField] private float glideLinearDamping = 5f;
        [SerializeField] private float glideAngleDamping = 0.005f;
        
        [Header("Layer Detection Settings")]
        public LayerMask groundLayer;
        public LayerMask waterLayer;
        
        [Header("Default Settings")]
        [SerializeField] private float constGroundRaycastDistance = 0.4f;
        [SerializeField] private float constGravity = 9.81f;
        [SerializeField] private float constLinearDamping = 0f;
        [SerializeField] private float constAngleDamping = 0.001f;

        private Rigidbody rb;
        private Animator anim;
        
        // Current State Values
        private bool isGrounded = true;
        private bool isSprinting = false;
        private bool isSwimming = false;
        private bool isInAir = false;
        private bool isGliding = false;

        private bool shouldRayCast = true;
        
        // Current State Float Values
        private Vector3 currentMoveDirection;
        private float currentSpeed;
        private float currentGravity;
        private float currentLinearDamping;
        private float currentAngleDamping;

        private float originalVFXRate = 0f;
        
        // Command Pattern Movement
        private MoveCommand? moveCommand = null;
        
        // Animation Cached Property Index
        private static readonly int Walk = Animator.StringToHash("Walk");
        private static readonly int IsInAir = Animator.StringToHash("isJump");
        // private static readonly int IsFall = Animator.StringToHash("isFall");
        private static readonly int IsInWater = Animator.StringToHash("isInWater");
        private static readonly int IsSwim = Animator.StringToHash("isSwim");
        private static readonly int IsFly = Animator.StringToHash("isFly");

        private void OnEnable()
        {
            inputReader.EnableGamePlayActionInputs();
            
            inputReader.MoveEvent += HandleMove;
            inputReader.MoveCanceledEvent += HandleMoveCanceled;
            inputReader.SprintEvent += HandleSprint;
            inputReader.SprintCanceledEvent += HandleSprintCanceled;
            inputReader.JumpEvent += HandleJump;
            inputReader.JumpCanceledEvent += HandleJumpCanceled;
            inputReader.GlideEvent += HandleGlide;
            inputReader.JumpCanceledEvent += HandleGlideCanceled;
        }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            anim = GetComponent<Animator>();
            
            currentSpeed = constMoveSpeed;
            currentAngleDamping = constAngleDamping;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if ((waterLayer.value & (1 << other.gameObject.layer)) != 0)
            {
                // waterLayerMask에 포함된 Layer일 때만 실행
                isSwimming = true;
                isInAir = false;
                isGliding = false;
                
                rb.useGravity = false;
                
                anim.SetInteger(Walk, 0);
                // anim.SetBool(IsFall, false);
                anim.SetBool(IsInAir, false);
                anim.SetBool(IsFly, false);
                anim.SetBool(IsInWater, true);
                
                rb.linearDamping = constLinearDamping;
                Physics.gravity = new Vector3(0, -constGravity, 0);
                
                if (moveCommand != null)
                    anim.SetBool(IsSwim, true);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if ((waterLayer.value & (1 << other.gameObject.layer)) != 0)
            {
                isSwimming = false;
                rb.useGravity = true;
                anim.SetBool(IsSwim, false);
                anim.SetBool(IsInWater, false);
            }
        }
        
        private void Update()
        {
            if (shouldRayCast)
            {
                RaycastHit groundHit;
                if (Physics.Raycast(transform.position, Vector3.down, out groundHit, constGroundRaycastDistance, groundLayer) && !isInAir)
                {
                    isGrounded = true;
                
                    if (isGliding)
                        HandleGlideCanceled();

                    if (isInAir)
                        HandleJumpCanceled();
                    
                    anim.SetBool(IsInAir, false);

                    // anim.SetBool(IsFall, false);

                    // check if still moving when hitting the ground
                    if (moveCommand != null)
                    {
                        if (isSprinting)
                            anim.SetInteger(Walk, 2);
                        else
                            anim.SetInteger(Walk, 1);   
                    }
                }
                else
                {
                    isGrounded = false;
                    anim.SetInteger(Walk, 0);
                }
            }
        }

        private void FixedUpdate()
        {
            if (moveCommand.HasValue)
            {
                if (moveCommand.Value.rotation.HasValue)
                    rb.MoveRotation(Quaternion.Slerp(rb.rotation, moveCommand.Value.rotation.Value, 5 * Time.fixedDeltaTime));

                if (moveCommand.Value.direction.HasValue)
                    rb.MovePosition(transform.position + moveCommand.Value.direction.Value * (moveCommand.Value.speed * Time.fixedDeltaTime));
            }
        }

        private void OnDisable()
        {
            inputReader.MoveEvent -= HandleMove;
            inputReader.MoveCanceledEvent -= HandleMoveCanceled;
            inputReader.SprintEvent -= HandleSprint;
            inputReader.SprintCanceledEvent -= HandleSprintCanceled;
            inputReader.JumpEvent -= HandleJump;
            inputReader.JumpCanceledEvent -= HandleJumpCanceled;
            inputReader.GlideEvent -= HandleGlide;
            inputReader.JumpCanceledEvent -= HandleGlideCanceled;
        }

        private void HandleMove(Vector2 direction)
        {
            if (isGliding && direction.magnitude > 0.1f)
                currentMoveDirection = new Vector3(direction.x, 0, direction.y).normalized;
            else
                currentMoveDirection = new Vector3(direction.x, 0, direction.y);
            
            float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            float angleDamping = 0f;
            angleDamping = Mathf.SmoothDampAngle(transform.eulerAngles.y, angle, ref angleDamping, currentAngleDamping);
            
            moveCommand = new MoveCommand
            {
                speed = currentSpeed,
                direction = currentMoveDirection,
                rotation = Quaternion.Euler(0, angleDamping, 0)
            };
            
            // Handle Animations
            if (isGrounded)
                anim.SetInteger(Walk, isSprinting ? 2 : 1);
            else if (isSwimming)
                anim.SetBool(IsSwim, true);
        }

        private void HandleMoveCanceled(Vector2 direction)
        {
            moveCommand = null;
            
            // Handle Animations
            if (isGrounded)
                anim.SetInteger(Walk, 0);
            else if (isSwimming)
                anim.SetBool(IsSwim, false);
        }

        private void HandleSprint()
        {
            currentSpeed = constSprintSpeed;
            isSprinting = true;

            if (moveCommand.HasValue)
            {
                moveCommand = new MoveCommand
                {
                    speed = currentSpeed,
                    direction = moveCommand.Value.direction,
                    rotation = moveCommand.Value.rotation
                };
                
                anim.SetInteger(Walk, 2);
            }
        }
        
        private void HandleSprintCanceled()
        {
            currentSpeed = constMoveSpeed;
            isSprinting = false;
            
            if (moveCommand.HasValue)
            {
                moveCommand = new MoveCommand
                {
                    speed = currentSpeed,
                    direction = moveCommand.Value.direction,
                    rotation = moveCommand.Value.rotation
                };
                
                anim.SetInteger(Walk, 1);
            }
        }

        private void HandleJump()
        {
            if ((isGrounded || isSwimming) && !isInAir && !isGliding)
            {
                isInAir = true;
                isGrounded = false;
                anim.SetInteger(Walk, 0);
                anim.SetBool(IsInAir, true);
                
                rb.AddForce(Vector3.up * constJumpSpeed, ForceMode.Impulse);

                StartCoroutine(DisableGroundRaycast());
            }
        }

        IEnumerator DisableGroundRaycast()
        {
            shouldRayCast = false;
            yield return new WaitForSeconds(0.2f);
            shouldRayCast = true;
        }
        
        private void HandleJumpCanceled()
        {
            if (isInAir)
            {
                isInAir = false;
                // anim.SetBool(IsJump, false);
            }
        }

        private void HandleGlide()
        {
            if (isInAir && !isGliding)
            {
                isGliding = true;
                isInAir = false; // isJumping = false
                currentAngleDamping = glideAngleDamping;
                
                // anim.SetBool(IsFall, false);
                anim.SetBool(IsFly, true);
                rb.linearDamping = glideLinearDamping;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, -2f, rb.linearVelocity.z);
                Physics.gravity = new Vector3(0, -glideGravity, 0);
            }
        }

        private void HandleGlideCanceled()
        {
            if (isGliding)
            {
                isGliding = false;
                currentAngleDamping = constAngleDamping;
                
                anim.SetBool(IsFly, false);
                rb.linearDamping = constLinearDamping;
                Physics.gravity = new Vector3(0, -constGravity, 0);
            }
        }
    }
}
