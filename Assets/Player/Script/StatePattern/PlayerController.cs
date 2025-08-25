//using Gamekit3D;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
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
    public Transform cameraTransform;

    public Animator animator;
    public Rigidbody rb;
    public Vector3 moveDirection;
    public bool isRunning;


    #region InputAction 변수
    private PlayerInput playerInput;
    private InputAction moveAction;            // WASD
    private InputAction sprintAction;          // Shift
    private InputAction jumpAction;            // Space
    #endregion

    #region Ground 체크 관련 변수
    [Header("Check isGrounded")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float sphereRadius = 0.2f;
    [SerializeField] private float raycastDistance = 0.5f;
    [SerializeField] private Transform[] groundCheckPoints;
    public bool isGrounded;
    #endregion

    #region 경사로 계산 관련 변수
    [Header("최대 경사 각도 검사")]
    [SerializeField] Transform raycastOrigin;
    [SerializeField] float maxSlopeAngle;

    [Header("경사로 회전")]
    [SerializeField] private AnimationCurve animCurve;
    [SerializeField] private float timer = 0.25f;
    private RaycastHit slopeHit;
    #endregion

    #region 점프 관련 변수
    [Header("Jump Force")]
    [SerializeField] private float jumpForce = 9f;
    private bool isJumping = false;
    private bool wasFalling = false;
    #endregion

    void Awake()
    {
        ChangeState(new RunningState());
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        #region PlayerInput
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        sprintAction = playerInput.actions["Run"];
        jumpAction = playerInput.actions["Jump"];

        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;

        sprintAction.started += OnSprint;
        sprintAction.performed += OnSprint; 
        sprintAction.canceled += OnSprint; 

        jumpAction.performed += OnJump;
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
    }

    void Update()
    {
        currentState?.Update(this);
    }

    void FixedUpdate()
    {
        currentState?.FixedUpdate(this);
    }

    public void ChangeState(IPlayerState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState.Enter(this);
    }

    void OnMove(InputAction.CallbackContext context)
    {
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
        // 지면에서만 점프 가능
        if (isGrounded)
            ChangeState(new JumpState());
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
                animator.SetBool("isFall", false);
                break;
            }
            else
            {
                isGrounded = false;

                if (rb.linearVelocity.y < -0.1f && !wasFalling)
                {
                    wasFalling = true;
                    animator.SetBool("isJump", false);
                    animator.SetBool("isFall", true);
                }
            }
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
}

