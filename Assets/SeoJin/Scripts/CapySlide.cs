using System;
using System.Text;
using UnityEngine;

public class CapySlide : MonoBehaviour
{
    private Rigidbody rb;
    private Camera mainCam;
    
    [Header("Force Settings")]
    [SerializeField] private float gravity = 20f;
    [SerializeField] private float horizontalControlForce = 5f;
    [SerializeField] private float verticalControlForce = 2f;

    [Header("Ground Check Settings")] 
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float groundCheckOffset = 0f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer = -1;
    private bool isGrounded;
    private Vector3 groundNormal;

    [Header("Velocity Settings")] 
    [SerializeField] private float maxZVelocity = 10f;
    [SerializeField] private float minZVelocity = 5f;

    [Header("FOV Settings")]
    [SerializeField] private float maxFOV = 80f;
    [SerializeField] private float minFOV = 60f;
    [SerializeField] private float fovChangeSpeed = 5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCam = Camera.main;
    }

    private void Update()
    {
        isGrounded = GroundCheck();
        ApplyAdjustedGravity();
        ControlPlayer();
        ClampZVelocity();
        ChangeFOV();
        
        

        UpdateDebugText();
    }

    private bool GroundCheck()
    {
        Vector3 origin = transform.position + Vector3.up * groundCheckOffset;
        if (Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
        {
            groundNormal = hit.normal;
            return true;
        }
        else
        {
            groundNormal = Vector3.up;
            return false;
        }
    }

    private void ApplyAdjustedGravity()
    {
        Vector3 gravityForce = Vector3.down * gravity;
        if (isGrounded)
        {
            Vector3 adjustedGravityForce = Vector3.ProjectOnPlane(gravityForce, groundNormal);
            rb.AddForce(adjustedGravityForce, ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(gravityForce, ForceMode.Acceleration);
        }
        
    }

    private void ClampZVelocity()
    {
        float zVelocity = rb.linearVelocity.z;
        zVelocity = Mathf.Clamp(zVelocity, minZVelocity, maxZVelocity);
        
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, zVelocity);
    }

    private void ControlPlayer()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        MoveHorizontally(horizontal);
        MoveVertically(vertical);
    }

    private void MoveHorizontally(float horizontal)
    {
        Vector3 forceDir = transform.right * horizontal;
        rb.linearVelocity += forceDir * horizontalControlForce;
    }

    private void MoveVertically(float vertical)
    {
        Vector3 forceDir = transform.forward * vertical;
        rb.AddForce(forceDir * verticalControlForce, ForceMode.Force);
    }


    private void ChangeFOV()
    {
        float zVelocity = rb.linearVelocity.z;
    
        float normalizedVelocity = Mathf.InverseLerp(minZVelocity, maxZVelocity, zVelocity);
    
        float targetFOV = Mathf.Lerp(minFOV, maxFOV, normalizedVelocity);
    
        mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, targetFOV, fovChangeSpeed * Time.deltaTime);
    }


    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 origin = transform.position + Vector3.up * groundCheckOffset;
            if (Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckDistance,
                    groundLayer))
            {
                Gizmos.DrawLine(origin, hit.point);
                Gizmos.DrawWireSphere(hit.point, groundCheckRadius);
            }
            else
            {
                Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
                Gizmos.DrawWireSphere(origin + Vector3.down * groundCheckDistance, groundCheckRadius);
            }

        }
    }

    private void UpdateDebugText()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"X Velocity : {rb.linearVelocity.x}\n");
        sb.Append($"Y Velocity : {rb.linearVelocity.y}\n");
        sb.Append($"Z Velocity : {rb.linearVelocity.z}");
        DebugCanvas.Instance.debugText.text = sb.ToString();
    }
}
