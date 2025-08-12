using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    private PlayerInput defaultAction;
    private Rigidbody rb;
    private Animator animator;
    private Vector3 InputKey;
    float Myfloat;
    private RaycastHit slopeHit;
    [SerializeField]  private AnimationCurve animCurve;
    [SerializeField] private float timer = 0.25f;


    private Vector3 moveDirection;
    public float walkSpeed = 3.5f;
    public float runSpeed = 7f;
    public float glideSpeed = 4.5f;
    public float maxSlopeAngle = 60;
    bool isRunning;

    [Header("레이어 구분")]
    public LayerMask groundLayer;
    public LayerMask waterLayerMask;

    [Header("빙판 설정")]
    public LayerMask iceLayerMask;
    public float iceSlideSpeed = 5.0f;       // 빙판에서 최대 속도
    public float iceFriction = 0.02f;        // 빙판 감속량(낮을수록 오래 미끄러짐)
    public float iceTurnSmooth = 0.35f; // 빙판에서의 회전 딜레이(더 크게!)
    [SerializeField] private bool isOnIce = false;
    private Vector3 slidingVelocity = Vector3.zero;

    private float raycastDistance = 0.5f;
    private bool isGrounded;

    private bool isJumping = false;
    private bool wasFalling = false;
    private float jumpForce = 6f;

    private bool isSwimming = false;
  

    private float glideGravity = 4.0f; 
    private float normalGravity = 9.0f;
    private float glideDrag = 5f;              
    private float normalDrag = 0f;            
    private bool isGliding = false;

    private float normalSmoothing = 0.1f;    
    private float glideSmoothing = 0.5f;      
    float currentSmoothing;

    private float glideTurnSpeed = 50f; // 초당 최대 회전 각도
    private float targetGlideAngle; // 목표 방향 각도
    private float currentGlideAngle; // 실제 캐릭터가 바라보는 각도

    public bool inWindZone = false;
    public GameObject windZone;

    private void Start()
    {
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

        

        if (isGliding)
            currentSmoothing = glideSmoothing; 
        else
            currentSmoothing = normalSmoothing;

        if (isGliding && isGrounded) StopGlide();

        

        // Ground check
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            isGrounded = true;
            isJumping = false;
            wasFalling = false;
            animator.SetBool("isFall", false);
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

    void FixedUpdate()
    {
        

        bool hasControl = (moveDirection !=  Vector3.zero);

        #region #빙판 이동
        if (isOnIce)
        {
            if (hasControl)
                slidingVelocity = Vector3.Lerp(slidingVelocity, moveDirection.normalized * iceSlideSpeed, 0.08f);
            else
                slidingVelocity = Vector3.Lerp(slidingVelocity, Vector3.zero, iceFriction);

            rb.MovePosition(transform.position + slidingVelocity * Time.fixedDeltaTime);

            if (slidingVelocity.sqrMagnitude > 0.02f)
            {
                float targetAngle = Mathf.Atan2(slidingVelocity.x, slidingVelocity.z) * Mathf.Rad2Deg;
                float smooth = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref Myfloat, 0.35f); // 딜레이↑
                transform.rotation = Quaternion.Euler(0, smooth, 0);
            }

            animator.SetInteger("Walk", (slidingVelocity.magnitude > 0.2f) ? 1 : 0);
            return;
        }
        #endregion

        #region #글라이딩
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
        #endregion

        #region #그냥 걷기
        else if (hasControl)
        {
            Move();
            
        }
        #endregion
        else
        {
            if (isSwimming)
                animator.SetBool("isSwim", false);
            else
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

    private Quaternion SurfaceAlignment()
    {
        Quaternion RotationRef = Quaternion.Euler(0, 0, 0);

        if (IsOnSlope())
        {
            Vector3 adjustedForward = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(adjustedForward, slopeHit.normal);
            RotationRef = Quaternion.Lerp(transform.rotation, targetRotation, animCurve.Evaluate(timer));
        }
        //Ray ray = new Ray(transform.position, -Vector3.up);
        //RaycastHit info = new RaycastHit();
        //Quaternion RotationRef = Quaternion.Euler(0, 0, 0);

        //if (Physics.Raycast(ray, out info, 5f, groundLayer))
        //{
        //    Vector3 adjustedForward = Vector3.ProjectOnPlane(moveDirection, info.normal).normalized;
        //    Quaternion targetRotation = Quaternion.LookRotation(adjustedForward, info.normal);
        //    RotationRef = Quaternion.Lerp(transform.rotation, targetRotation, animCurve.Evaluate(timer));

        //    //RotationRef = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(Vector3.up, info.normal), animCurve.Evaluate(timer));
        //}

        return RotationRef;
    }

    protected void Move()
    {
        Quaternion RotationRef = SurfaceAlignment();
        float speed = isRunning ? runSpeed : walkSpeed;

        float angle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
        float smooth = Mathf.SmoothDampAngle(transform.eulerAngles.y, angle, ref Myfloat, currentSmoothing);
        transform.rotation = Quaternion.Euler(RotationRef.eulerAngles.x, smooth, RotationRef.eulerAngles.z);

        bool isOnSlope = IsOnSlope();
        Vector3 velocity = isOnSlope ? AdjustDirectionToSlope(moveDirection) : moveDirection;
        Vector3 gravity = isOnSlope ? Vector3.zero : Vector3.down * Mathf.Abs(rb.linearVelocity.y);
        Vector3 counterMovement = new Vector3(-rb.linearVelocity.x, 0, -rb.linearVelocity.z);

        //rb.linearVelocity = velocity * speed + gravity;

        if (isOnSlope)
        {
            rb.linearVelocity = new Vector3(velocity.x * speed, rb.linearVelocity.y, velocity.z * speed);
        }
        else
        {
            rb.linearVelocity = velocity * speed + gravity;
        }


        if (isSwimming)
            animator.SetBool("isSwim", true);
        else
            animator.SetInteger("Walk", isRunning ? 2 : 1);


       //rb.MovePosition(transform.position + moveDirection * speed * Time.deltaTime);

    }

    protected Vector3 AdjustDirectionToSlope(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
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

    void OnJump()
    {
        if(isGrounded)
        {
            isJumping = true;
            animator.SetBool("isJump", true);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

    }

    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        if (isGliding)
        {
            if (input.magnitude > 0.1f)
                moveDirection = new Vector3(input.x, 0, input.y).normalized;
            // 입력이 없을 땐 방향 유지
        }
        else
        {
            moveDirection = new Vector3(input.x, 0f, input.y);
        }
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
