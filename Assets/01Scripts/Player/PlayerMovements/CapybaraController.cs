using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Capybara
{
    public struct MoveCommand
    {
        public float speed;
        public Vector3? direction;
        public Quaternion? rotation;
    }
    
    public class CapybaraController : MonoBehaviour
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
        [SerializeField] private float glideAngleDamping = 0.5f;
        
        [Header("Layer Detection Settings")]
        public LayerMask groundLayer;
        public LayerMask waterLayer;
        
        [Header("Default Settings")]
        [SerializeField] private float constGroundRaycastDistance = 0.5f;
        [SerializeField] private float constGravity = 9.81f;
        [SerializeField] private float constLinearDamping = 0f;
        [SerializeField] private float constAngleDamping = 0.001f;

        private Rigidbody rb;
        private Animator anim;
        
        // Current State Values
        private bool isGrounded = true;
        private bool isSprinting = false;
        private bool isSwimming = false;
        private bool isJumping = false;
        private bool isGliding = false;
        
        // Current State Float Values
        private Vector3 currentMoveDirection;
        private float currentSpeed;
        private float currentGravity;
        private float currentLinearDamping;
        private float currentAngleDamping;
        
        // Command Pattern Movement
        private MoveCommand? moveCommand = null;
        
        // Animation Cached Property Index
        private static readonly int Walk = Animator.StringToHash("Walk");
        private static readonly int IsJump = Animator.StringToHash("isJump");
        private static readonly int IsFall = Animator.StringToHash("isFall");
        private static readonly int IsInWater = Animator.StringToHash("isInWater");
        private static readonly int IsSwim = Animator.StringToHash("isSwim");
        private static readonly int IsFly = Animator.StringToHash("isFly");

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            anim = GetComponent<Animator>();
            
            inputReader.Initialize();
            
            inputReader.MoveEvent += HandleMove;
            inputReader.MoveCanceledEvent += HandleMoveCanceled;
            inputReader.SprintEvent += HandleSprint;
            inputReader.SprintCanceledEvent += HandleSprintCanceled;
            inputReader.JumpEvent += HandleJump;
            inputReader.JumpCanceledEvent += HandleJumpCanceled;
            inputReader.GlideEvent += HandleGlide;
            inputReader.GlideCanceledEvent += HandleGlideCanceled;
            
            currentSpeed = constMoveSpeed;
            currentAngleDamping = constAngleDamping;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if ((waterLayer.value & (1 << other.gameObject.layer)) != 0)
            {
                // waterLayerMask에 포함된 Layer일 때만 실행
                isSwimming = true;
                rb.useGravity = false;
                anim.SetBool(IsInWater, true);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if ((waterLayer.value & (1 << other.gameObject.layer)) != 0)
            {
                isSwimming = false;
                rb.useGravity = true;
                anim.SetBool(IsInWater, false);
            }
        }
        
        private void Update()
        {
            RaycastHit groundHit;
            if (Physics.Raycast(transform.position, Vector3.down, out groundHit, constGroundRaycastDistance, groundLayer))
            {
                isGrounded = true;
                
                if (isGliding)
                    HandleGlideCanceled();
                
                if (isJumping)
                    isJumping = false;
                
                anim.SetBool(IsFall, false);
            }
            else
            {
                isGrounded = false;

                if (rb.linearVelocity.y < -0.1f)
                {
                    anim.SetBool(IsJump, false);
                    anim.SetBool(IsFall, true);
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

        private void HandleMove(Vector2 direction)
        {
            if (isGliding && direction.magnitude > 0.1f)
            {
                float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                float angleDamping = 0f;
                angleDamping = Mathf.SmoothDampAngle(transform.eulerAngles.y, angle, ref angleDamping, currentAngleDamping);

                Debug.Log(angleDamping);
                
                moveCommand = new MoveCommand
                {
                    speed = currentSpeed,
                    direction = new Vector3(direction.x, 0, direction.y).normalized,
                    rotation = Quaternion.Euler(0, angleDamping, 0)
                };
            }
            else
            {
                float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                float angleDamping = 0f;
                angleDamping = Mathf.SmoothDampAngle(transform.eulerAngles.y, angle, ref angleDamping, currentAngleDamping);

                Debug.Log(currentAngleDamping);
                
                moveCommand = new MoveCommand
                {
                    speed = currentSpeed,
                    direction = new Vector3(direction.x, 0, direction.y),
                    rotation = Quaternion.Euler(0, angleDamping, 0)
                };
            }
            
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
        }
        
        private void HandleSprintCanceled()
        {
            currentSpeed = constMoveSpeed;
            isSprinting = false;
        }

        private void HandleJump()
        {
            if (isGrounded)
            {
                isJumping = true;
                anim.SetBool(IsJump, true);
                rb.AddForce(Vector3.up * constJumpSpeed, ForceMode.Impulse);
            }
        }
        
        private void HandleJumpCanceled()
        {
            if (isJumping)
            {
                isJumping = false;
                anim.SetBool(IsJump, false);
            }
        }

        private void HandleGlide()
        {
            if (!isGrounded)
            {
                isGliding = true;
                currentAngleDamping = glideAngleDamping;
                
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
