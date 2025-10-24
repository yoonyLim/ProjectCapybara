using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Numerics;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Capybara
{
    public class CapybaraControllerFinalScene : MonoBehaviour
    {
        [SerializeField] private CapybaraInputReader inputReader;
        
        [Header("Layer Detection Settings")]
        public LayerMask groundLayer;
        
        [Header("Default Settings")]
        [SerializeField] private float constGroundRaycastDistance = 0.4f;
        
        private Rigidbody rb;
        private Animator anim;
        
        [Header("Camera Settings")]
        [SerializeField] private CinemachineCamera mainCineCam;
        
        [Header("Cinematic Settings")]
        [SerializeField] PlayableDirector introTimeline;
        
        [Header("UI Anim Settings")]
        [SerializeField] private Animator flashUIAnim;
        [SerializeField] private Animator fadeUIAnim;
        [SerializeField] private Image blackoutImg;
        
        private Vector3 originalPosition;
        private Vector3 destinationPosition = new Vector3(1040.1431f, 110.729973f, 2560f);
        private float totalDistance;
        
        // Current State Values
        private bool isGrounded = true;
        private float speed = 5f;
        private bool isMovable = true;
        private bool shouldFloat = false;
        private bool shouldFall = false;
        private bool isDuringCinematic = false;
        
        private Vector2 moveInput;
        
        // Animation Cached Property Index
        private static readonly int Walk = Animator.StringToHash("Walk");
        private static readonly int IsInAir = Animator.StringToHash("isFly");
        private static readonly int IsOnGround = Animator.StringToHash("isGrounded");
        private static readonly int FlashIn = Animator.StringToHash("FlashIn");
        private static readonly int FlashOut = Animator.StringToHash("FlashOut");
        private static readonly int FadeIn = Animator.StringToHash("FadeIn");
        private static readonly int FadeOut = Animator.StringToHash("FadeOut");
        private static readonly int HappyCapy = Animator.StringToHash("HappyCapy");

        private void OnEnable()
        {
            inputReader.EnableGamePlayActionInputs();
            
            inputReader.MoveEvent += HandleMove;
            inputReader.MoveCanceledEvent += HandleMoveCanceled;

            SpawnPointManager.OnPlayerRespawned += ResetController;
        }

        private void ResetController()
        {
            if (rb)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            anim = GetComponent<Animator>();
        }

        private void Start()
        {
            originalPosition = transform.position;
            totalDistance = Vector3.Distance(originalPosition, destinationPosition);
            introTimeline.Play();
        }
        
        private void Update()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, constGroundRaycastDistance, groundLayer))
            {
                isGrounded = true;
                anim.SetBool(IsInAir, false);
                anim.SetBool(IsOnGround, true);
            }

            if (shouldFloat && transform.position.y < 113)
            {
                float yPos = CapyHelperLibrary.FInterpTo(transform.position.y, 113, Time.deltaTime, 1f);
                transform.position = new Vector3(transform.position.x, yPos, transform.position.z);
            }

            if (!isDuringCinematic)
            {
                // blackout img
                float curDistance = Vector3.Distance(originalPosition, transform.position);
                Color tempColor = blackoutImg.color;
                tempColor.a = Mathf.Clamp01(curDistance / totalDistance);
                blackoutImg.color = new Color(tempColor.r, tempColor.g, tempColor.b, tempColor.a);
            }
        }

        private void FixedUpdate()
        {
            if (moveInput.y > 0 && isMovable)
            {
                rb.linearVelocity = Vector3.forward * speed;
                anim.SetInteger(Walk, 1);
            }
            else
            {
                anim.SetInteger(Walk, 0);
            }
        }

        private void OnDisable()
        {
            inputReader.MoveEvent -= HandleMove;
            inputReader.MoveCanceledEvent -= HandleMoveCanceled;
        }

        private void HandleMove(Vector2 direction)
        {
            moveInput = direction;
            DualSenseInputManager.Instance.RumbleControllerForDuration(0.1f, 0.1f);
        }

        private void HandleMoveCanceled(Vector2 direction)
        {
            moveInput = Vector2.zero;
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

        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;

            if (newSpeed <= 0.3f)
            {
                Invoke(nameof(SetIsDuringCinematic), 5.5f);
            }
        }

        private void SetIsDuringCinematic()
        {
            isDuringCinematic = true;
            blackoutImg.color = new Color(blackoutImg.color.r, blackoutImg.color.g, blackoutImg.color.b, 0);
        }

        public void SetMoveable(bool val)
        {
            isMovable = val;
        }

        public void FloatAndFall()
        {
            shouldFloat = true;
            rb.useGravity = false;
            
            Invoke(nameof(ShouldFall), 5f);
        }

        private void ShouldFall()
        {
            shouldFloat = false;
            rb.useGravity = true;
        }

        public void MeetGrandma()
        {
            speed = 15f;
            anim.SetTrigger(HappyCapy);
            isMovable = true;
        }
    }
}
