using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{

    // 카메라
    [SerializeField] private Transform cameraTransform;

    [SerializeField] private float sphereRadius = 0.2f;
    [SerializeField] private float raycastDistance = 0.5f;
    [SerializeField] private Transform[] groundCheckPoints;

    [Header("최대 경사 각도 검사")]
    [SerializeField] Transform raycastOrigin;
    [SerializeField] float maxSlopeAngle;

    private PlayerInput defaultAction;
    private Rigidbody rb;
    private Animator animator;
    private Vector3 InputKey;
    float Myfloat;
    private RaycastHit slopeHit;

    [Header("경사로 회전")]
    [SerializeField]  private AnimationCurve animCurve;
    [SerializeField] private float timer = 0.25f;


    private Vector3 moveDirection;
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 7f;
    private float glideSpeed = 4.5f;
    bool isRunning;

    [Header("레이어 구분")]
    public LayerMask groundLayer;
    public LayerMask waterLayerMask;

    
    public LayerMask iceLayerMask;
    [SerializeField] private bool isOnIce = false;
    private Vector3 slidingVelocity = Vector3.zero;
    private bool isGrounded;

    private bool isJumping = false;
    private bool wasFalling = false;
    [SerializeField] private float jumpForce = 9f;

    private bool isSwimming = false;
  

    private float glideGravity = 4.0f;
    [SerializeField] private float normalGravity = 9f;
    private float glideDrag = 5f;              
    private float normalDrag = 0f;            
    private bool isGliding = false;

    private float normalSmoothing = 0.1f;    
    private float glideSmoothing = 0.5f;      
    float currentSmoothing;

    private float glideTurnSpeed = 50f; // 초당 최대 회전 각도
    private float targetGlideAngle; // 목표 방향 각도
    private float currentGlideAngle; // 실제 캐릭터가 바라보는 각도

    private bool inWindZone = false;
    private GameObject windZone;


    private void Start()
    {
        cameraTransform = Camera.main.transform;
        defaultAction = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        
        var glideAction = defaultAction.actions["Glide"];
        glideAction.performed += ctx => StartGlide();
        glideAction.canceled += ctx => StopGlide();

        var sprintAction = defaultAction.actions["Sprint"];
        sprintAction.performed += ctx => StartSprint();
        sprintAction.canceled += ctx => StopSprint();
    }

    void Update()
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


        if (isGliding)
            currentSmoothing = glideSmoothing;
        if (isOnIce)
            currentSmoothing = 0.5f;
        else
            currentSmoothing = normalSmoothing;

        if (isGliding && isGrounded) StopGlide();
    }


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


    void FixedUpdate()
    {
        

        bool hasControl = (moveDirection !=  Vector3.zero);
        Move();

        if (isGliding)
        {
            if (moveDirection.sqrMagnitude > 0.01f)
                targetGlideAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

            float newAngle = Mathf.MoveTowardsAngle(
                transform.eulerAngles.y,  
                targetGlideAngle,          
                glideTurnSpeed * Time.deltaTime 
            );
            transform.rotation = Quaternion.Euler(0, newAngle, 0);

            Vector3 forward = transform.forward;
            rb.MovePosition(transform.position + forward * glideSpeed * Time.deltaTime);
        }
        else if (hasControl)
        {
            
            
        }
        else
        {

            if (isSwimming)
                animator.SetBool("isSwim", false);
            else if(isGrounded)
                animator.SetInteger("Walk", 0);

        }

        // 글라이딩 시 이동 코드
        if (isGliding)
        {
            Vector3 glideMove = new Vector3(moveDirection.x, 0, moveDirection.z);
            rb.AddForce(glideMove * 7f, ForceMode.Acceleration);
        }

        if(inWindZone)
        {
            Vector3 windDirection = windZone.transform.up.normalized;
            rb.AddForce(windDirection * windZone.GetComponent<WindZone>().strength);

            float maxFallSpeed = -2f;
            if (rb.linearVelocity.y < maxFallSpeed)
            {
                Vector3 vel = rb.linearVelocity;
                vel.y = maxFallSpeed;
                rb.linearVelocity = vel;
            }
        }


    }

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

    private Quaternion SurfaceAlignment()
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

    protected void Move()
    {
        Quaternion RotationRef = SurfaceAlignment();
        float speed = isRunning ? runSpeed : walkSpeed;

        if (moveDirection.magnitude > 0.01f)
        {
            float angle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float smooth = Mathf.SmoothDampAngle(transform.eulerAngles.y, angle, ref Myfloat, currentSmoothing);
            transform.rotation = Quaternion.Euler(RotationRef.eulerAngles.x, smooth, RotationRef.eulerAngles.z);
        }

        bool isOnSlope = IsOnSlope();
        Vector3 velocity = CalculateNextFrameGroundAngle(speed) < maxSlopeAngle ? moveDirection : Vector3.zero;
        Vector3 gravity = Vector3.down * Mathf.Abs(rb.linearVelocity.y);

        if(isOnIce)
        {
            rb.AddForce(velocity * 0.4f, ForceMode.Impulse);
        }
        else
        {
            if (isOnSlope && isGrounded)
            {
                
                velocity = AdjustDirectionToSlope(moveDirection);
                gravity = Vector3.zero;
                rb.useGravity = false;
                if (moveDirection.magnitude < 0.01f)
                {
                    // 입력이 없으면 슬로프에서도 고정
                    rb.linearVelocity = Vector3.zero;
                }
                else
                {
                    rb.linearVelocity = velocity * speed + gravity;
                }
            }
            else
            {
                rb.useGravity = true;
                Vector3 currentVelocity = rb.linearVelocity;
                Vector3 targetVelocity = new Vector3(velocity.x * speed, currentVelocity.y, velocity.z * speed); // y 유지

                rb.linearVelocity = targetVelocity;       
            }
        }

        

        if (isSwimming)
            animator.SetBool("isSwim", true);
        else if(isGrounded)
            animator.SetInteger("Walk", isRunning ? 2 : 1);


    }

    protected Vector3 AdjustDirectionToSlope(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }


    private float CalculateNextFrameGroundAngle(float moveSpeed)
    {
        var nextFramePlayerPosition =
                           raycastOrigin.position + moveDirection * moveSpeed * Time.fixedDeltaTime;

        if (Physics.Raycast(nextFramePlayerPosition, Vector3.down, out RaycastHit hitInfo,
                            0.5f, groundLayer))
            return Vector3.Angle(Vector3.up, hitInfo.normal);
        return 0f;
    }

    void OnJump()
    {
        if(isGrounded)
        {
            isJumping = true;
            animator.SetBool("isJump", true);
            //rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            Vector3 jumpDirection = slopeHit.normal;
            rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);
        }

    }

    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        // 카메라 기준으로 이동 방향 변환
        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0;
        camForward.Normalize();
        Vector3 camRight = cameraTransform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 desiredMove = camForward * input.y + camRight * input.x;
        moveDirection = desiredMove.normalized;
    }

    void OnTriggerEnter(Collider other)
    {
        if ((waterLayerMask.value & (1 << other.gameObject.layer)) != 0)
        {
            // waterLayerMask에 포함된 Layer일 때만 실행
            isSwimming = true;
            animator.SetBool("isInWater", true);
            rb.useGravity = false;
        }

        if (other.gameObject.tag == "windZone")
        {
            windZone = other.gameObject;
            inWindZone = true;
        }

        if (other.gameObject.tag == "jumpPad")
        {
            rb.AddForce(Vector3.up * 12f, ForceMode.Impulse);
        }

    }

    private void OnCollisionEnter(Collision other)
    {
        if (((1 << other.gameObject.layer) & iceLayerMask.value) != 0)
            isOnIce = true;
    }

    private void OnCollisionExit(Collision other)
    {
        if (((1 << other.gameObject.layer) & iceLayerMask.value) != 0)
        {
            isOnIce = false;
            slidingVelocity = Vector3.zero;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if ((waterLayerMask.value & (1 << other.gameObject.layer)) != 0)
        {
            isSwimming = false;
            animator.SetBool("isInWater", false);
            rb.useGravity = true;
        }

        if(other.gameObject.tag == "windZone")
        {
            inWindZone = false;
        }
    }

    void StartSprint()
    {
        isRunning = true;
    }

    void StopSprint()
    {
        isRunning = false;
    }
    void StartGlide()
    {
        // 땅에 붙었으면 글라이딩 안됨
        if (isGrounded) return;
        if (isGliding) return;

        isGliding = true;
        animator.SetBool("isFly", true); // 자유롭게 원하는 파라미터로
        rb.linearDamping = glideDrag;
        Physics.gravity = new Vector3(0, -glideGravity, 0);
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, -2f, rb.linearVelocity.z); // 낙하 초기속도 완화. 값 조절 가능
    }

    void StopGlide()
    {
        if (!isGliding) return;

        isGliding = false;
        animator.SetBool("isFly", false);
        rb.linearDamping = normalDrag;
        Physics.gravity = new Vector3(0, -normalGravity, 0);
    }
}
