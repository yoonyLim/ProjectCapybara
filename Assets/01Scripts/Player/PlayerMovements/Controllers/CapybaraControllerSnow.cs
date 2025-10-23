using System;
using System;
using System.Runtime.CompilerServices;
using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

namespace Capybara
{
    public class CapybaraControllerSnow : MonoBehaviour
    {
        [SerializeField] private CapybaraInputReader inputReader;
        
        // SKI CHANGE: We need the camera's orientation for relative controls.
        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private ParticleSystem windSpeedParticles;
        [SerializeField] private ParticleSystem collectOrbsWindSpeedParticles;
        [SerializeField] private GameObject Clouds;

        [Header("Snow Level Specifics")] [SerializeField]
        private float breakDistance = 1200f;

        // SKI CHANGE: Movement is now controlled by physics forces, not constant speeds.
        [Header("Skiing Settings")]
        [SerializeField] private float acceleration = 30f; // How quickly you speed up downhill
        [SerializeField] private float maxSpeed = 50f; // The maximum speed you can reach
        [SerializeField] private float turnSpeed = 10f; // How sharply you can steer
        [SerializeField] private float friction = 0.1f; // How much you slow down on flat ground
        [SerializeField] private float brakingPower = 25f; // How effectively you can slow down by turning uphill
        [SerializeField] [Range(0f, 1f)] private float airControl = 0.3f; // How much you can steer while airborne

        [Header("Jump Settings")]
        [SerializeField] private float constJumpSpeed = 6f;
        
        [Header("Layer Detection Settings")]
        public LayerMask groundLayer;
        public LayerMask snowRampLayer;
        // public LayerMask waterLayer;
        
        [Header("Default Settings")]
        [SerializeField] private float constGroundRaycastDistance = 0.4f;
        
        private Rigidbody rb;
        private Animator anim;
        
        [Header("Camera Settings")]
        [SerializeField] private CinemachineCamera mainCineCam;
        [SerializeField] private CinemachineCamera collectOrbCineCam;
        
        [Header("Cinematic Settings")]
        [SerializeField] PlayableDirector collectOrbsTimeline;
        
        // Current State Values
        private bool isGrounded = true;
        private bool isSwimming = false;
        private bool isWindZoned = false;
        private bool isOverBreakDistance = false;
        
        private Vector2 moveInput;

        private Vector3 slopeNormal;
        // private Vector3 slopeForward;
        
        // Animation Cached Property Index
        // private static readonly int Walk = Animator.StringToHash("Walk");
        private static readonly int IsInAir = Animator.StringToHash("isFly");
        private static readonly int IsOnIce = Animator.StringToHash("isIced");

        private void OnEnable()
        {
            inputReader.EnableGamePlayActionInputs();
            
            inputReader.MoveEvent += HandleMove;
            inputReader.MoveCanceledEvent += HandleMoveCanceled;
            inputReader.JumpEvent += HandleJump;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            anim = GetComponent<Animator>();
            
            // SKI CHANGE: If the camera transform isn't assigned, find the main camera.
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("SnowGround"))
            {
                Debug.Log("snow ramp Collision Enter");
                Quaternion targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(rb.linearVelocity, slopeNormal).normalized, slopeNormal);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f));
            }
                
        }
        
        private void Update()
        {
            // Ground Check
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, constGroundRaycastDistance, groundLayer))
            {
                isGrounded = true;
                slopeNormal = hit.normal;
                anim.SetBool(IsInAir, false);
            }
            else
            {
                isGrounded = false;
                slopeNormal = Vector3.up; // Assume flat when airborne
                anim.SetBool(IsInAir, true);
            }
            
            // Animation: Set speed based on the rigidbody's actual velocity magnitude.
            // float currentSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
            // anim.SetFloat(Walk, currentSpeed / maxSpeed);

            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, constGroundRaycastDistance, snowRampLayer))
            {
                DualSenseInputManager.Instance.RumbleControllerForDuration(0.1f, 0.1f);
                
                if (!rb.useGravity)
                    rb.useGravity = true;
                
                var mainParticleSystem = windSpeedParticles.main;
                mainParticleSystem.startSpeed = rb.linearVelocity.magnitude;
                anim.SetBool(IsOnIce, true);
                anim.SetBool(IsInAir, false);
            }
            else
            {
                var mainParticleSystem = windSpeedParticles.main;
                mainParticleSystem.startSpeed = 0f;
                anim.SetBool(IsOnIce, false);

                if (!isGrounded)
                {
                    anim.SetBool(IsInAir, true);
                }
            }
            
            // collect orbs
            if (transform.position.z >= breakDistance && !isOverBreakDistance)
            {
                /*CinemachineFollow follwComp = cineCam.GetComponent<CinemachineFollow>();
                if (follwComp)
                    follwComp.FollowOffset = new Vector3(0f, 10f, 0f);*/
                
                mainCineCam.enabled = false;
                collectOrbCineCam.enabled = true;
                
                isOverBreakDistance = true;
                isWindZoned = false;
                Instantiate(Clouds, transform.position + Vector3.down * 100 + Vector3.forward * 200, Quaternion.identity);
                rb.useGravity = false;
                // rb.linearVelocity = new Vector3(0f, -20f, 0f);
                collectOrbsTimeline.Play();
            }

            if (isOverBreakDistance)
            {
                Debug.Log("over distance effect");
                // set particles
                var mainParticleSystem = collectOrbsWindSpeedParticles.main;
                mainParticleSystem.startSpeed = rb.linearVelocity.magnitude;
            }
            else
            {
                var mainParticleSystem = collectOrbsWindSpeedParticles.main;
                mainParticleSystem.startSpeed = 0;
            }
        }

        public void SetIsWindZoned(bool val)
        {
            isWindZoned = val;
        }

        private void FixedUpdate()
        {
            // --- WINDZONED MOVEMENT ---
            if (isWindZoned)
                return;
            
            // --- SKIING MOVEMENT ---
            
            // 1. DETERMINE FORWARD DIRECTION RELATIVE TO CAMERA AND SLOPE
            Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 cameraRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            Vector3 worldInputDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;
            
            // 2. APPLY FORCES
            if (isGrounded)
            {
                /*// GRAVITY FORCE: The main force pushing us down the slope.
                Vector3 gravityForce = Vector3.ProjectOnPlane(new Vector3(0, -constGravity, 0), slopeNormal);
                rb.AddForce(gravityForce, ForceMode.Acceleration);
                
                // STEERING FORCE: Apply force based on player input to turn.
                float turnAngle = Vector3.SignedAngle(slopeForward, worldInputDirection, Vector3.up);
                Vector3 steeringForce = transform.right * turnAngle * turnSpeed;
                rb.AddForce(steeringForce, ForceMode.Acceleration);

                // BRAKING & FRICTION: Slow down if turning uphill or on flat ground.
                float angleToDownhill = Vector3.Angle(slopeForward, gravityForce.normalized);
                if (angleToDownhill > 90f) // We are facing uphill
                {
                    float brakingForceMagnitude = (angleToDownhill - 90f) / 90f;
                    rb.AddForce(-rb.velocity.normalized * brakingPower * brakingForceMagnitude, ForceMode.Acceleration);
                }
                rb.AddForce(-rb.velocity * friction, ForceMode.Acceleration);*/
                
                rb.AddForce(worldInputDirection * acceleration, ForceMode.Acceleration);
            }
            else // Air Control
            {
                rb.AddForce(worldInputDirection * acceleration * airControl, ForceMode.Acceleration);
            }
            
            // 3. CAP SPEED
            Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (currentVelocity.magnitude > maxSpeed)
            {
                Vector3 cappedVelocity = currentVelocity.normalized * maxSpeed;
                rb.linearVelocity = new Vector3(cappedVelocity.x, rb.linearVelocity.y, cappedVelocity.z);
            }

            // 4. ROTATION: Align the character with the slope and direction of movement.
            if (rb.linearVelocity.magnitude > 0.5f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(rb.linearVelocity, slopeNormal).normalized, slopeNormal);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f));
            }
        }

        private void OnDisable()
        {
            inputReader.MoveEvent -= HandleMove;
            inputReader.MoveCanceledEvent -= HandleMoveCanceled;
            inputReader.JumpEvent -= HandleJump;
        }

        private void HandleMove(Vector2 direction)
        {
            moveInput = direction;
        }

        private void HandleMoveCanceled(Vector2 direction)
        {
            moveInput = Vector2.zero;
        }

        private void HandleJump()
        {
            if (isGrounded)
            {
                rb.AddForce(Vector3.up * constJumpSpeed, ForceMode.Impulse);
            }
        }

        public void SetOrbCollectionLinearVelocity()
        {
            rb.linearVelocity = new Vector3(0f, -20f, 0f);
        }
    }
}
