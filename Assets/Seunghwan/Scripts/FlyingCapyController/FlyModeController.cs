using System.Collections;
using Capybara;
using Unity.Cinemachine;
using UnityEngine;

public class FlyModeController : MonoBehaviour
{
    private Rigidbody capyRigidBody;
    private readonly float forwardFlightStrength = 150f;
    private readonly float upwardFlightStrength = 200f;
    private readonly float yawRotationSpeed = 100f;
    private readonly float obstacleHitRotationSpeed = 800f;
    private readonly float maxMeshRoll = 25f;
    private readonly float maxMeshPitch = 40f;
    private readonly float bounceStrength = 40f;
    private readonly float maxSpeed = 100f;

    private readonly float normalLinearDamping = 3f;
    private readonly float obstacleHitLinearDamping = 1f;
    private readonly float obstacleHitDuration = 1f;
    private readonly float camShakeDuration = 0.175f;

    private bool shouldSpinLeft = false;
    private float currentHitRotationSpeed;

    private FlyModeState state = FlyModeState.Normal;
    private float obstacleHitEnterTime = 0f;
    private bool blockForwardAcceleration = false;
    
    private Vector2 moveInput;

    private readonly int Alpha = Shader.PropertyToID("_Alpha");

    [SerializeField] private Transform meshTransform;
    [SerializeField] private Animator capyAnimator;
    [SerializeField] private Animator birdAnimator;

    private readonly int hitAnimTrigger = Animator.StringToHash("Hit");
    private readonly int eyesSpinAnimState = Animator.StringToHash("Eyes_Spin");
    private readonly int eyesIdleAnimState = Animator.StringToHash("Eyes_Idle");
    
    [SerializeField] private CinemachineOrbitalFollow cmOrbitalFollow;
    [SerializeField] private CinemachineBasicMultiChannelPerlin cmPerlin;
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private CapybaraInputReader capybaraInputReader;

    [SerializeField] private Material speedEffectMaterial;

    [SerializeField] private BirdHitSound hitSoundComponent;

    enum FlyModeState
    {
        Normal,
        ObstacleHit
    }
    
    private void Awake()
    {
        capyRigidBody = GetComponent<Rigidbody>();
        capyRigidBody.maxLinearVelocity = maxSpeed;
        capyRigidBody.linearDamping = normalLinearDamping;
    }

    private void OnEnable()
    {
        capybaraInputReader.EnableGamePlayActionInputs();
        capybaraInputReader.MoveEvent += OnMove;
        capybaraInputReader.MoveCanceledEvent += OnMoveCanceled;
    }

    private void OnDisable()
    {
        capybaraInputReader.MoveEvent -= OnMove;
        capybaraInputReader.MoveCanceledEvent -= OnMoveCanceled;
        speedEffectMaterial.SetFloat(Alpha, 0f);
    }

    private void OnMove(Vector2 input)
    {
        moveInput = input;
    }
    

    private void OnMoveCanceled(Vector2 input)
    {
        moveInput = Vector2.zero;
    }

    private void Update()
    {
        ApplyMeshLocalRotation();

        float targetY = 25f - moveInput.y * 30f;
        cmOrbitalFollow.VerticalAxis.Value = FInterpTo(cmOrbitalFollow.VerticalAxis.Value , targetY, Time.deltaTime, 2f);

        float targetEffectAlpha =
            0.1f * Mathf.Clamp01(capyRigidBody.linearVelocity.magnitude / capyRigidBody.maxLinearVelocity);
        speedEffectMaterial.SetFloat(Alpha, targetEffectAlpha);
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case FlyModeState.Normal:
                NormalStateFixedUpdate();
                break;
            case FlyModeState.ObstacleHit:
                ObstacleHitStateFixedUpdate();
                break;
            default:
                break;
        }
    }

    void NormalStateFixedUpdate()
    {
        Vector3 rbYawForward = capyRigidBody.transform.forward;
        rbYawForward.y = 0;
        rbYawForward.Normalize();
        
        if (!blockForwardAcceleration)
        {
            Vector3 forwardForce = rbYawForward * forwardFlightStrength;
            capyRigidBody.AddForce(forwardForce, ForceMode.Acceleration);
        }
        
        if (Mathf.Abs(moveInput.y) > Mathf.Epsilon)
        {
            capyRigidBody.AddForce(Vector3.up * (moveInput.y * upwardFlightStrength), ForceMode.Acceleration);
        }

        Quaternion yawDelta = Quaternion.Euler(0f , moveInput.x * yawRotationSpeed * Time.fixedDeltaTime, 0f);
        capyRigidBody.MoveRotation(capyRigidBody.rotation * yawDelta);
    }

    void ObstacleHitStateFixedUpdate()
    {
        
        if (Time.time - obstacleHitEnterTime > obstacleHitDuration)
        {
            ChangeState(FlyModeState.Normal);
        }
    }

    IEnumerator ShakeCameraCoroutine()
    {
        cmPerlin.AmplitudeGain = 10f;
        yield return new WaitForSeconds(camShakeDuration);
        cmPerlin.AmplitudeGain = 0f;
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
        
        if (other.gameObject.CompareTag("FlyingObstacle"))
        {
            hitSoundComponent.PlayHitSound();
            
            DualSenseInputManager.Instance.RumbleControllerForDuration(1f, 0.1f);
            Vector3 currentVelocityDir = capyRigidBody.linearVelocity.normalized;
            Vector3 impulseDir = other.impulse.normalized;

            Vector3 cross = Vector3.Cross(currentVelocityDir, impulseDir);
            shouldSpinLeft = cross.y < 0f;
            
            capyRigidBody.AddForce(meanNormal * 60f, ForceMode.Impulse);
            
            ChangeState(FlyModeState.ObstacleHit);
            
        }
        else
        {
            StartCoroutine(BlockForwardAccelerationCoroutine(1f));
            DualSenseInputManager.Instance.RumbleControllerForDuration(0.3f, 0.1f);
            capyRigidBody.AddForce(meanNormal * bounceStrength, ForceMode.Impulse);
        }
        
    }

    private void NormalStateEnter()
    {
        capyAnimator.CrossFadeInFixedTime("Eyes_Idle", 0.15f, capyAnimator.GetLayerIndex("Face Layer"));
        birdAnimator.CrossFadeInFixedTime(eyesIdleAnimState, 0.15f, birdAnimator.GetLayerIndex("Face Layer"));
        capyRigidBody.linearDamping = normalLinearDamping;
    }

    private void ObstacleHitStateEnter()
    {
        capyAnimator.CrossFadeInFixedTime("Eyes_Spin", 0.15f, capyAnimator.GetLayerIndex("Face Layer"));
        birdAnimator.CrossFadeInFixedTime(eyesSpinAnimState, 0.15f, birdAnimator.GetLayerIndex("Face Layer"));
        StartCoroutine(ShakeCameraCoroutine());
        capyRigidBody.linearDamping = obstacleHitLinearDamping;
        currentHitRotationSpeed = obstacleHitRotationSpeed;
        obstacleHitEnterTime = Time.time;
    }
    private void ApplyMeshLocalRotation()
    {
        switch (state)
        {
            case FlyModeState.Normal:
            {
                float targetRoll = -moveInput.x * maxMeshRoll;
                float targetPitch = -moveInput.y * maxMeshPitch;
                Quaternion rollRotation = Quaternion.AngleAxis(targetRoll, Vector3.forward);
                Quaternion pitchRotation = Quaternion.AngleAxis(targetPitch, Vector3.right);
                Quaternion targetMeshRotation = pitchRotation * rollRotation;
                meshTransform.localRotation = QInterpTo(meshTransform.localRotation, targetMeshRotation,
                    Time.deltaTime, 3f);
                break;
            }
            case FlyModeState.ObstacleHit:
            {
                currentHitRotationSpeed = FInterpTo(currentHitRotationSpeed, 150f, Time.deltaTime, 0.5f);
                if (shouldSpinLeft)
                {
                    meshTransform.Rotate(0, currentHitRotationSpeed * Time.deltaTime, 0, Space.Self);
                }
                else
                {
                    meshTransform.Rotate(0, -currentHitRotationSpeed * Time.deltaTime, 0, Space.Self);
                }
                break;
            }
            default:
                break;
        }
        
    }

    IEnumerator BlockForwardAccelerationCoroutine(float duration)
    {
        blockForwardAcceleration = true;
        yield return new WaitForSeconds(duration);
        blockForwardAcceleration = false;
    }

    private void OnStateEnter(FlyModeState inState)
    {
        switch (inState)
        {
            case FlyModeState.Normal:
                NormalStateEnter();
                break;
            case FlyModeState.ObstacleHit:
                ObstacleHitStateEnter();
                break;
            default:
                break;
        }
    }

    private void OnStateExit(FlyModeState inState)
    {
        switch (inState)
        {
            case FlyModeState.Normal:
                break;
            case FlyModeState.ObstacleHit:
                break;
            default:
                break;
        }
    }
    
    private void ChangeState(FlyModeState newState)
    {
        if (newState == state) return;
        OnStateExit(state);
        state = newState;
        OnStateEnter(newState);
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
