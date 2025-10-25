using System;
using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Numerics;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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
        private OrbCollectionCheckTrigger orbCollectionTrigger;
        
        [Header("Camera Settings")]
        [SerializeField] private CinemachineCamera mainCineCam;
        [SerializeField] private CinemachineCamera collectOrbCineCam;
        [SerializeField] private CinemachineCamera finalCineCam;
        
        [Header("Cinematic Settings")]
        [SerializeField] PlayableDirector collectOrbsTimeline;
        [SerializeField] private GameObject finalRamp;
        [SerializeField] PlayableDirector orbCollectionFailedTimeline;
        [SerializeField] private LoadingManager loadingManager;
        
        [Header("UI Anim Settings")]
        [SerializeField] private Animator flashUIAnim;
        [SerializeField] private Animator fadeUIAnim;
        
        // Current State Values
        private bool isGrounded = true;
        private bool isSwimming = false;
        private bool isWindZoned = false;
        private bool isOverBreakDistance = false;
        private bool isLevelSuccessful = false;
        
        private Vector2 moveInput;
        private Vector3 slopeNormal;
        // private Vector3 slopeForward;
        
        // for orbs collection failure
        private Vector3 lastKnownPos;
        private Quaternion lastKnownRot;
        private GameObject finalFall;
        
        // Animation Cached Property Index
        private static readonly int Walk = Animator.StringToHash("Walk");
        private static readonly int IsInAir = Animator.StringToHash("isFly");
        private static readonly int IsOnGround = Animator.StringToHash("isGrounded");
        private static readonly int IsOnIce = Animator.StringToHash("isIced");
        private static readonly int FlashIn = Animator.StringToHash("FlashIn");
        private static readonly int FlashOut = Animator.StringToHash("FlashOut");
        private static readonly int FadeIn = Animator.StringToHash("FadeIn");
        private static readonly int FadeOut = Animator.StringToHash("FadeOut");

        [SerializeField] AudioSource footstepSource;
        [SerializeField] AudioClip snowSlideClip;
        public void PlaySnowSlideSound()
        {
            footstepSource.PlayOneShot(snowSlideClip);
        }
        
        private void OnEnable()
        {
            inputReader.EnableGamePlayActionInputs();
            
            inputReader.MoveEvent += HandleMove;
            inputReader.MoveCanceledEvent += HandleMoveCanceled;
            inputReader.JumpEvent += HandleJump;

            SpawnPointManager.OnPlayerRespawned += ResetController;
        }

        private void ResetController()
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
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
            anim.SetInteger(Walk, 0);
            
            // Ground Check
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, constGroundRaycastDistance, groundLayer))
            {
                isGrounded = true;
                slopeNormal = hit.normal;
                anim.SetBool(IsInAir, false);
                anim.SetBool(IsOnGround, true);
            }
            else
            {
                isGrounded = false;
                slopeNormal = Vector3.up; // Assume flat when airborne

                if (!isLevelSuccessful)
                {
                    anim.SetBool(IsOnGround, false);
                    anim.SetBool(IsInAir, true);
                }
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
                if (!isLevelSuccessful)
                {
                    var mainParticleSystem = windSpeedParticles.main;
                    mainParticleSystem.startSpeed = 0f;
                    
                    anim.SetBool(IsOnIce, false);
                    
                    footstepSource.Stop();

                    if (!isGrounded && !isLevelSuccessful)
                    {
                        anim.SetBool(IsInAir, true);
                    }
                }
            }
        }

        public void SetIsWindZoned(bool val)
        {
            isWindZoned = val;
        }

        private void FixedUpdate()
        {
            if (isLevelSuccessful)
            {
                // rb.linearVelocity = Vector3.forward * 25 + Vector3.down * 50;
                rb.useGravity = true;
                rb.AddForce(Vector3.forward, ForceMode.Acceleration);
                
                Vector3 currentSlidingVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                if (currentSlidingVelocity.magnitude > 20)
                {
                    Vector3 cappedVelocity = currentSlidingVelocity.normalized * maxSpeed;
                    rb.linearVelocity = new Vector3(cappedVelocity.x, rb.linearVelocity.y, cappedVelocity.z);
                }
            }
            
            // collect orbs
            if (transform.position.z >= breakDistance && !isOverBreakDistance)
            {
                rb.linearVelocity = Vector3.zero;
                
                lastKnownPos = transform.position;
                lastKnownRot = transform.rotation;
                
                mainCineCam.enabled = false;
                collectOrbCineCam.enabled = true;
                
                isOverBreakDistance = true;
                isWindZoned = false;
                finalFall = Instantiate(Clouds, transform.position + Vector3.down * 100 + Vector3.back * 100, Quaternion.identity);
                orbCollectionTrigger = finalFall.GetComponentInChildren<OrbCollectionCheckTrigger>();

                if (finalFall && orbCollectionTrigger)
                {
                    orbCollectionTrigger.OnOrbsCollected += OrbsCollectedSuccessfully;
                }
                
                rb.useGravity = false;
                // rb.linearVelocity = new Vector3(0f, -20f, 0f);
                collectOrbsTimeline.Play();
            }

            if (isOverBreakDistance)
            {
                // set particles
                var mainParticleSystem = collectOrbsWindSpeedParticles.main;
                mainParticleSystem.startSpeed = rb.linearVelocity.magnitude;
            }
            else
            {
                var mainParticleSystem = collectOrbsWindSpeedParticles.main;
                mainParticleSystem.startSpeed = 0;
            }
            
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
                rb.AddForce(worldInputDirection * acceleration, ForceMode.Acceleration);
            }
            else // Air Control
            {
                worldInputDirection = (Vector3.forward * moveInput.y + Vector3.right * moveInput.x).normalized;
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
            
            if (orbCollectionTrigger)
                orbCollectionTrigger.OnOrbsCollected -= OrbsCollectedSuccessfully;
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

        public void OrbsCollectedSuccessfully()
        {
            footstepSource.enabled = false;
            
            rb.linearVelocity = Vector3.zero;
            finalRamp.SetActive(true);
            PlayFlashIn();
            Invoke(nameof(MoveToSceneTransitionRamp), 0.5f);
        }

        [SerializeField] private SoundHandler bgmHandler;

        private void MoveToSceneTransitionRamp()
        {
            transform.position = new Vector3(1041f, 151f, 2521f);
            transform.rotation = Quaternion.identity;
            collectOrbCineCam.enabled = false;
            finalCineCam.enabled = true;
            isLevelSuccessful = true;
            anim.SetBool(IsOnIce, true);

            if (bgmHandler)
            {
                bgmHandler.FadeOutSong(10f);
            }
            
            Invoke(nameof(PlayFlashOut), 1.0f);
        }

        private void PlayFlashIn()
        {
            flashUIAnim.SetTrigger(FlashIn);
        }

        private void PlayFlashOut()
        {
            flashUIAnim.SetTrigger(FlashOut);
        }

        private void PlayFadeIn()
        {
            fadeUIAnim.SetTrigger(FadeIn);
        }

        private void PlayFadeOut()
        {
            fadeUIAnim.SetTrigger(FadeOut);
        }

        public void LevelCompleted()
        {
            rb.useGravity = false;
            finalCineCam.Follow = null;
            Invoke(nameof(PlayFlashIn), 0.5f);
            Invoke(nameof(NextLevel), 1.0f);
        }

        private void NextLevel()
        {
            loadingManager.LoadScene(3);
        }

        public void OrbCollectionFailed()
        {
            PlayFadeIn();
            
            Invoke(nameof(ResetOnOrbCollectionFailure), 0.5f);
        }

        private void ResetOnOrbCollectionFailure()
        {
            PlayFadeOut();
            
            rb.linearVelocity = Vector3.zero;
            
            transform.position = lastKnownPos;
            transform.rotation = lastKnownRot;
            
            mainCineCam.enabled = false;
            collectOrbCineCam.enabled = true;
            
            orbCollectionTrigger.OnOrbsCollected -= OrbsCollectedSuccessfully;
            Destroy(finalFall);
            
            finalFall = Instantiate(Clouds, transform.position + Vector3.down * 100, Quaternion.identity);
            orbCollectionTrigger = finalFall.GetComponentInChildren<OrbCollectionCheckTrigger>();
            
            if (finalFall && orbCollectionTrigger)
            {
                orbCollectionTrigger.OnOrbsCollected += OrbsCollectedSuccessfully;
            }
            
            rb.useGravity = false;
            
            orbCollectionFailedTimeline.Play();
        }
    }
}
