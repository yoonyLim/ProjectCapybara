using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class FlyModeController : MonoBehaviour
{
    private Rigidbody capyRigidBody;
    private readonly float forwardFlightStrength = 150f;
    private readonly float yawRotationSpeed = 100f;
    private readonly float maxMeshRoll = 25f;
    private readonly float maxMeshPitch = 35f;
    private readonly float bounceStrength = 50f;
    
    private Vector2 moveInput;

    [SerializeField] private Transform meshTransform;
    [SerializeField] private Animator capyAnimator;
    [SerializeField] private Animator birdAnimator;

    private readonly int hitAnimTrigger = Animator.StringToHash("Hit");
    
    [SerializeField] private CinemachineOrbitalFollow cmOrbitalFollow;
    
    private void Awake()
    {
        capyRigidBody = GetComponent<Rigidbody>();
        capyRigidBody.maxLinearVelocity = 100f;
        capyRigidBody.linearDamping = 3f;
    }

    private void Update()
    {
        moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        
        ApplyMeshLocalRotation();

        float targetY = -1f * moveInput.y * 3f;
        cmOrbitalFollow.TargetOffset.y = FInterpTo(cmOrbitalFollow.TargetOffset.y, targetY, Time.deltaTime, 2f);
    }

    private void FixedUpdate()
    {
        Vector3 rbYawForward = capyRigidBody.transform.forward;
        rbYawForward.y = 0;
        rbYawForward.Normalize();
        
        Vector3 forwardForce = rbYawForward * forwardFlightStrength;
        capyRigidBody.AddForce(forwardForce, ForceMode.Acceleration);

        if (Mathf.Abs(moveInput.y) > Mathf.Epsilon)
        {
            capyRigidBody.AddForce(Vector3.up * (moveInput.y * forwardFlightStrength), ForceMode.Acceleration);
            
        }

        if (Math.Abs(moveInput.x) > Mathf.Epsilon)
        {
            Quaternion yawDelta = Quaternion.Euler(0, moveInput.x * yawRotationSpeed * Time.fixedDeltaTime, 0);
            capyRigidBody.MoveRotation(capyRigidBody.rotation * yawDelta);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.contactCount == 0) return;
        
        capyAnimator.SetTrigger(hitAnimTrigger);
        birdAnimator.SetTrigger(hitAnimTrigger);

        Vector3 meanNormal = Vector3.zero;
        foreach (var contact in other.contacts)
        {
            meanNormal += contact.normal;
        }
        meanNormal = (meanNormal / other.contactCount).normalized;
        capyRigidBody.AddForce(meanNormal * bounceStrength, ForceMode.Impulse);
    }

    void ApplyMeshLocalRotation()
    {
        float targetRoll = -moveInput.x * maxMeshRoll;
        float targetPitch = -moveInput.y * maxMeshPitch;
        Quaternion rollRotation = Quaternion.AngleAxis(targetRoll, Vector3.forward);
        Quaternion pitchRotation = Quaternion.AngleAxis(targetPitch, Vector3.right);
        Quaternion targetMeshRotation = pitchRotation * rollRotation;
        meshTransform.localRotation = QInterpTo(meshTransform.localRotation, targetMeshRotation,
            Time.deltaTime, 6f);
    }

    private Quaternion QInterpTo(Quaternion current, Quaternion target, float deltaTime, float interpSpeed)
    {
        if (interpSpeed <= 0f || Quaternion.Angle(current, target) < 0.01f) return target;
        
        return Quaternion.Slerp(current, target, Mathf.Clamp01(interpSpeed * deltaTime));
    }

    private float FInterpTo(float current, float target, float deltaTime, float interpSpeed)
    {
        if (interpSpeed <= 0f) return target;

        float dist = target - current;
        if (Mathf.Abs(dist) < 0.01f) return target;
        float deltaMove = dist * Mathf.Clamp01(deltaTime * interpSpeed);
        return current + deltaMove;
    }
}
