//using Gamekit3D;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Windows;


public interface IPlayerState
{
    void Enter(PlayerController player);
    void Exit(PlayerController player);
    void HandleInput(PlayerController player, InputAction.CallbackContext context);
    void Update(PlayerController player);
    void FixedUpdate(PlayerController player);
}


public class PlayerController : MonoBehaviour
{
    private IPlayerState currentState;
    [HideInInspector] public Transform cameraTransform;
    private bool isInWindZone = false;
    [SerializeField] private PlayerHapticEvent playerHapticEvent;

    public Animator animator;
    public Rigidbody rb;

    [HideInInspector] public Vector3 moveDirection;
    [HideInInspector] public bool isRunning;
    [HideInInspector] public Vector2 LastMoveInput { get; private set; }
    [HideInInspector] public Vector2 MoveInput { get; private set; }
    [HideInInspector] public Vector3 platformVelocity;
    #region InputAction 변수
    private PlayerInput playerInput;
    private InputAction moveAction;            // WASD
    private InputAction sprintAction;          // Shift
    private InputAction jumpAction;            // Space
    private InputAction glideAction;           // Space 홀딩
    #endregion

    #region Ground 체크 관련 변수
    [Header("Check isGrounded")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float sphereRadius = 0.2f;
    [SerializeField] private float raycastDistance = 0.5f;
    [SerializeField] private Transform[] groundCheckPoints;
    [HideInInspector] public bool isGrounded;
    #endregion

    #region 경사로 계산 관련 변수
    [Header("최대 경사 각도 검사")]
    [SerializeField] Transform raycastOrigin;
    [SerializeField] float maxSlopeAngle;

    [Header("경사로 회전")]
    [SerializeField] private AnimationCurve animCurve;
    [SerializeField] private float timer = 0.25f;
    [HideInInspector] public RaycastHit slopeHit;
    #endregion

    #region 점프 관련 변수
    [Header("Jump Force")]
    public float jumpForce = 7f;
    //[HideInInspector]
    public bool isJumping = false;
    private bool wasFalling = false;
    [HideInInspector] public float airSpeed = 5f;
    [HideInInspector] public float airAcceleration = 20f;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTimeDuration = 0.15f; // 코요테 시간 (0.1 ~ 0.2초 추천)
    public float coyoteTimeCounter;
    #endregion

    #region Ice 관련 변수
    [Header("Ice Layer 체크")]
    public LayerMask iceLayer;
    [HideInInspector] public bool isOnIce;
    #endregion

    #region Glide 관련 변수
    [Header("글라이드 유지 시간")]
    public float glideDuration = 2.5f;

    [Header("Glide 이동 변수")]
    public float glideSpeed = 7f;
    public float glideTurnSpeed = 0.5f;
    public float glideGravity = 4f;
    public float glideDrag = 5f;

    public float normalGravity = 9.81f;
    [HideInInspector] public float normalDrag = 0f;
    public bool glideLocked = false;
    #endregion

    #region DustLand 생성 변수
    [Header("DustLand 생성 속도")]
    [SerializeField] private float dustSpawnVel = -8.0f;
    public bool canSpawnDustLand = false;
    #endregion

    #region 리스폰 변수
    [SerializeField] private float fallDeathYLevel = -50f;
    #endregion

    private MountController mountController;
    void Awake()
    {
        mountController = GetComponent<MountController>();
        ChangeState(new RunningState());
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        #region PlayerInput
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        sprintAction = playerInput.actions["Run"];
        jumpAction = playerInput.actions["Jump"];
        glideAction = playerInput.actions["Glide"];

        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;

        sprintAction.started += OnSprint;
        sprintAction.performed += OnSprint; 
        sprintAction.canceled += OnSprint; 

        jumpAction.performed += OnJump;

        glideAction.started += OnGlide;
        glideAction.performed += OnGlide;
        glideAction.canceled += OnGlide;
        #endregion
    }
    void OnDisable()
    {
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;
        sprintAction.started -= OnSprint;
        sprintAction.performed -= OnSprint;
        sprintAction.canceled -= OnSprint;
        jumpAction.performed -= OnJump;
        glideAction.started -= OnGlide;
        glideAction.performed -= OnGlide;
        glideAction.canceled -= OnGlide;
    }

    void Update()
    {
        CheckGround();
        MoveInput = moveAction.ReadValue<Vector2>();
        currentState?.Update(this);

        if (rb.linearVelocity.y <= dustSpawnVel)
        {
            canSpawnDustLand = true;
        }

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTimeDuration;
        }
        else
        {
            // 공중에 있으면 코요테 시간 감소
            coyoteTimeCounter -= Time.deltaTime;
        }


        CheckForFallDeath();
    }

    void FixedUpdate()
    {
        currentState?.FixedUpdate(this);
    }

    public void ChangeState(IPlayerState newState)
    {
        Debug.Log($"상태 변경: {currentState?.GetType().Name} -> {newState.GetType().Name}");

        if (currentState is GlideState && !(newState is GlideState))
        {
            // Glide 상태에서 벗어나는 경우
            mountController?.OnPlayerGlide(false);
        }

        currentState?.Exit(this);
        currentState = newState;
        currentState.Enter(this);

        if (newState is JumpState)
        {
            mountController?.OnPlayerJump();
        }
        else if (newState is GlideState)
        {
            mountController?.OnPlayerGlide(true);
        }
    }

    void OnMove(InputAction.CallbackContext context)
    {
        LastMoveInput = context.ReadValue<Vector2>();
        currentState?.HandleInput(this, context);
    }

    void OnSprint(InputAction.CallbackContext context)
    {
        // Shift 누르면 달리기, 떼면 해제
        if (context.canceled) isRunning = false;
        else isRunning = true;
    }

    void OnJump(InputAction.CallbackContext context)
    {

        if (coyoteTimeCounter > 0f && !isJumping)
        {
            
            ChangeState(new JumpState());
            coyoteTimeCounter = 0f; // 점프하면 즉시 시간 초기화
        }

    }

    void OnGlide(InputAction.CallbackContext context)
    {
        // 시작/유지: 공중에서만 글라이드 진입
        if (context.performed && context.interaction is HoldInteraction)
        {
            if (!isGrounded)
            {
                ChangeState(new GlideState());
            }
            return;
        }

        // 취소: 상태 해제
        if (context.canceled && currentState is GlideState)
        {
            ChangeState(new RunningState());
            //if (isGrounded)
            //    ChangeState(new RunningState());
            //else
            //{
            //    glideLocked = true;
            //    ChangeState(new JumpState());
            //}
        }
    }

    public bool IsOnIceGround()
    {
        // 플레이어 발밑으로 레이쏴서 iceLayer만 맞는지 확인
        return Physics.Raycast(transform.position, Vector3.down, out _, raycastDistance + sphereRadius, iceLayer);
    }
    /// <summary>
    /// 플레이어가 땅에 닿아있는지 체크하는 함수
    /// 3개의 SphereCast 중 하나라도 땅에 닿아있다면 isGrounde = true
    /// </summary>
    public void CheckGround()
    {
        RaycastHit hit;
        foreach (var point in groundCheckPoints)
        {
            if (Physics.SphereCast(point.position, sphereRadius, Vector3.down, out hit, raycastDistance, groundLayer))
            {
                isGrounded = true;
                isJumping = false;
                wasFalling = false;
                glideLocked = false;

                if (!isInWindZone)
                {
                    animator.SetBool("isFly", false);
                }

                animator.SetBool("isGrounded", true);
                break;
            }
            else
            {
                isGrounded = false;
                animator.SetBool("isGrounded", false);
                if (rb.linearVelocity.y < -0.1f && !wasFalling)
                {
                    wasFalling = true;

                }
            }
        }

        isOnIce = IsOnIceGround();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("jumpPad") || other.CompareTag("windZone"))
        {
            isInWindZone = true;
            animator.SetBool("isGrounded", false);
            animator.SetBool("isFly", true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("jumpPad") || other.CompareTag("windZone"))
        {
            isInWindZone = false;
        }
    }

    //SphereCast Gizmo 그리는 코드
    void OnDrawGizmosSelected()
    {
        if (groundCheckPoints == null) return;

        Gizmos.color = Color.green;
        foreach (var point in groundCheckPoints)
        {
            Vector3 start = point.position;
            Vector3 end = start + Vector3.down * raycastDistance;
            // 구의 궤적 그리기
            Gizmos.DrawWireSphere(start, sphereRadius);
            Gizmos.DrawWireSphere(end, sphereRadius);
            // 사이 선으로 연결
            Gizmos.DrawLine(start, end);
        }
    }

    #region 경사 계산 함수

    // 플레이어가 경사 위에 있는 지 확인하는 코드
    // 플레이어 아래로 rayCast를 쏴서 angle이 0이 아니면 isOnSlope = true
    public bool IsOnSlope()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, out slopeHit, raycastDistance, groundLayer))
        {
            var angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle != 0f && angle < maxSlopeAngle;
        }
        return false;
    }

    // 경사 위에서의 각도를 계산하는 코드
    // 평지에서는 Quaternion.Euler(0, 0, 0)로 설정
    public Quaternion SurfaceAlignment()
    {
        Quaternion RotationRef = Quaternion.Euler(0, 0, 0);

        if (IsOnSlope())
        {
            Vector3 adjustedForward = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(adjustedForward, slopeHit.normal);
            RotationRef = Quaternion.Lerp(transform.rotation, targetRotation, animCurve.Evaluate(timer));
        }

        return RotationRef;
    }

    //이거 뭐더라 기억안남
    public Vector3 AdjustDirectionToSlope(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    // 경사 각도 체크 함수
    public float CalculateNextFrameGroundAngle(float moveSpeed)
    {
        var nextFramePlayerPosition =
                           raycastOrigin.position + moveDirection * moveSpeed * Time.fixedDeltaTime;

        if (Physics.Raycast(nextFramePlayerPosition, Vector3.down, out RaycastHit hitInfo,
                            0.5f, groundLayer))
            return Vector3.Angle(Vector3.up, hitInfo.normal);
        return 0f;
    }

    #endregion

    #region 리스폰 함수
    private void CheckForFallDeath()
    {
        if (transform.position.y < fallDeathYLevel)
        {
            Respawn();
        }
    }

    // 리스폰을 처리하는 함수
    public void Respawn()
    {
        Debug.Log("플레이어가 마지막 체크포인트에서 리스폰됩니다.");

        // Rigidbody의 속도를 초기화하여 떨어지던 가속도를 없애줍니다.
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Checkpoint 스크립트에 저장된 마지막 활성 위치로 플레이어를 즉시 이동시킵니다.
        // 순간이동 시 발생할 수 있는 물리적 오류를 방지하기 위해 잠시 CharacterController나 Rigidbody를 비활성화했다가 켜는 것이 더 안정적일 수 있습니다.
        transform.position = RespawnPoint.LastActivatedRespawnPpointPosition;

        // 여기에 체력 초기화 등 리스폰 시 필요한 다른 로직을 추가할 수 있습니다.
    }

    #endregion
}

