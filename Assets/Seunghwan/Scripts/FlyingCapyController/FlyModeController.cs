using System;
using System.Collections;
using System.Collections.Generic;
using Capybara;
using DistantLands.Cozy;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = System.Random;

public class FlyModeController : MonoBehaviour
{
    public event Action OnJumpKeyPressed;
    public event Action OnWeatherVolumeTriggerEnter;
    
    private Rigidbody capyRigidBody;
    private readonly float forwardFlightStrength = 100f;
    private readonly float upwardFlightStrength = 150f;
    private readonly float yawRotationSpeed = 130f;
    private readonly float obstacleHitRotationSpeed = 800f;
    private readonly float maxMeshRoll = 25f;
    private readonly float maxMeshPitch = 40f;
    private readonly float bounceStrength = 40f;
    private readonly float maxSpeed = 80f;

    private readonly float normalLinearDamping = 1f;
    private readonly float obstacleHitLinearDamping = 1f;
    private readonly float obstacleHitDuration = 1f;
    private readonly float camShakeDuration = 0.175f;

    private bool shouldSpinLeft = false;
    private float currentHitRotationSpeed;

    private FlyModeState state = FlyModeState.Normal;
    private float obstacleHitEnterTime = 0f;
    private bool blockForwardAcceleration = false;
    
    private Vector2 moveInput;

    private bool dodgeAnimLeft = true;

    private readonly int Alpha = Shader.PropertyToID("_Alpha");

    [SerializeField] private Transform meshRoot;
    [SerializeField] private Animator capyAnimator;
    [SerializeField] private Animator birdAnimator;
    [SerializeField] Animator meshAnimator;

    private readonly int hitAnimTrigger = Animator.StringToHash("Hit");
    private readonly int eyesSpinAnimState = Animator.StringToHash("Eyes_Spin");
    private readonly int eyesIdleAnimState = Animator.StringToHash("Eyes_Idle");
    
    private readonly int leftDodgeRoll = Animator.StringToHash("LeftDodgeRoll");
    private readonly int rightDodgeRoll = Animator.StringToHash("RightDodgeRoll");
    
    [SerializeField] private CinemachineOrbitalFollow cmOrbitalFollow;
    [SerializeField] private CinemachineBasicMultiChannelPerlin cmPerlin;
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private CapybaraInputReader capybaraInputReader;

    [SerializeField] private Material speedEffectMaterial;

    [SerializeField] private BirdHitSound hitSoundComponent;
    
    [SerializeField] private List<SkinnedMeshRenderer> meshRenderers;

    private float targetEffectAlpha = 0f;

    public bool DodgedLightning { get; set; }

    private float dodgedLightningTime = 0f;
    private bool dodgedLightningSlowmo = false;

    public Volume globalVolume;
    private Vignette vignette;

    private float lightningHitTime;
    private float lightningHitStateDuration = 2f;

    public float ActualImpactTime { get; set; }
    public bool CheckLightning { get; set; }

    public enum FlyModeState
    {
        Normal,
        ObstacleHit,
        LightningHit,
    }
    
    private void Awake()
    {
        capyRigidBody = GetComponent<Rigidbody>();
        capyRigidBody.maxLinearVelocity = maxSpeed;
        capyRigidBody.linearDamping = normalLinearDamping;

        globalVolume.profile.TryGet(out vignette);


    }

    private void OnEnable()
    {
        BirdLevelMusic.instance.PlayFirstPartMusic();
        capybaraInputReader.EnableGamePlayActionInputs();
        capybaraInputReader.MoveEvent += OnMove;
        capybaraInputReader.MoveCanceledEvent += OnMoveCanceled;
        capybaraInputReader.JumpEvent += OnJump;
    }

    private void OnDisable()
    {
        capybaraInputReader.MoveEvent -= OnMove;
        capybaraInputReader.MoveCanceledEvent -= OnMoveCanceled;
        capybaraInputReader.JumpEvent -= OnJump;
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

    private void OnJump()
    {
        OnJumpKeyPressed?.Invoke();
    }

    IEnumerator PlayVignetteEffect()
    {
        float increaseElapsedTime = 0f;
        while (increaseElapsedTime < 0.2f)
        {
            increaseElapsedTime += Time.deltaTime;
            vignette.intensity.value = Mathf.Lerp(0f, 0.6f, increaseElapsedTime / 0.2f);
            yield return null;
        }

        vignette.intensity.value = 0.4f;
        
        float decreaseElapsedTime = 0f;
        while (decreaseElapsedTime < 0.2f)
        {
            decreaseElapsedTime += Time.deltaTime;
            vignette.intensity.value = Mathf.Lerp(0.6f, 0f, decreaseElapsedTime / 0.2f);
            yield return null;
        }

        vignette.intensity.value = 0f;
    }
    
    private void Update()
    {
        float targetY = 25f - moveInput.y * 20f;
        cmOrbitalFollow.VerticalAxis.Value = FInterpTo(cmOrbitalFollow.VerticalAxis.Value , targetY, Time.deltaTime, 3f);

        float speedRatio = capyRigidBody.linearVelocity.magnitude / capyRigidBody.maxLinearVelocity;
        targetEffectAlpha = 0.2f * Mathf.Clamp01(1f / (1f- 0.8f) * (speedRatio - 0.8f));
        speedEffectMaterial.SetFloat(Alpha, targetEffectAlpha);

        

        if (dodgedLightningSlowmo)
        {
            if (Time.unscaledTime - dodgedLightningTime > 0.2f)
            {
                Time.timeScale = 1f;
                dodgedLightningSlowmo = false;
            }
        }
    }

    private void LateUpdate()
    {
        ApplyMeshLocalRotation();
    }

    private void FixedUpdate()
    {
        if (CheckLightning)
        {
            if (ActualImpactTime - Time.fixedTime < 0f)
            {
                CheckLightning = false;

                if (DodgedLightning)
                {
                    DodgedLightning = false;
                    Time.timeScale = 0.3f;
                    dodgedLightningTime = Time.fixedUnscaledTime;
                    dodgedLightningSlowmo = true;
                    PlayLightningDodgeAnimation();
                    StartCoroutine(PlayVignetteEffect());
                }
                else
                {
                    ChangeState(FlyModeState.LightningHit);
                }
            }

        }
        
        switch (state)
        {
            case FlyModeState.Normal:
                NormalStateFixedUpdate();
                break;
            case FlyModeState.ObstacleHit:
                ObstacleHitStateFixedUpdate();
                break;
            case FlyModeState.LightningHit:
                LightningHitFixedUpdate();
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
        if (Time.fixedTime - obstacleHitEnterTime > obstacleHitDuration)
        {
            ChangeState(FlyModeState.Normal);
        }
    }

    void LightningHitFixedUpdate()
    {
        float elapsedTime = Time.fixedTime - lightningHitTime;
        if (elapsedTime > lightningHitStateDuration)
        {
            ChangeState(FlyModeState.Normal);
        }

        float magnitude = Mathf.Lerp(0.2f, 0f, elapsedTime / lightningHitStateDuration);
        meshRoot.localPosition = UnityEngine.Random.insideUnitSphere * magnitude;

        float targetAlpha = Mathf.Lerp(1f, 0f, elapsedTime / lightningHitStateDuration);
        foreach (var renderer in meshRenderers)
        {
            renderer.materials[1].SetFloat("_Alpha", targetAlpha);
        }
    }

    IEnumerator ShakeCameraCoroutine()
    {
        cmPerlin.AmplitudeGain = 10f;
        yield return new WaitForSeconds(camShakeDuration);
        cmPerlin.AmplitudeGain = 0f;
    }
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out LoadingTrigger loadingTrigger))
        {
            GetComponentInChildren<BirdWingSound>().BlockSound = true;
        }

        if (other.TryGetComponent(out CozyVolume cozyVolume))
        {
            BirdLevelMusic.instance.PlaySecondPartMusic();
            OnWeatherVolumeTriggerEnter?.Invoke();
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

    private void LightningHitStateEnter()
    {
        lightningHitTime = Time.time;

        foreach (var renderer in meshRenderers)
        {
            renderer.materials[1].SetFloat("_Alpha", 1f);
        }
    }

    private void PlayLightningDodgeAnimation()
    {
        int animHash = dodgeAnimLeft ? leftDodgeRoll : rightDodgeRoll;
        dodgeAnimLeft = !dodgeAnimLeft;
        meshAnimator.CrossFadeInFixedTime(animHash, 0.15f);
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
                meshRoot.localRotation = QInterpTo(meshRoot.localRotation, targetMeshRotation,
                    Time.deltaTime, 3f);
                break;
            }
            case FlyModeState.ObstacleHit:
            {
                currentHitRotationSpeed = FInterpTo(currentHitRotationSpeed, 150f, Time.deltaTime, 0.5f);
                if (shouldSpinLeft)
                {
                    meshRoot.Rotate(0, currentHitRotationSpeed * Time.deltaTime, 0, Space.Self);
                }
                else
                {
                    meshRoot.Rotate(0, -currentHitRotationSpeed * Time.deltaTime, 0, Space.Self);
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
            case FlyModeState.LightningHit:
                LightningHitStateEnter();
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
            case FlyModeState.LightningHit:
            {
                meshRoot.localPosition = Vector3.zero;
                foreach (var renderer in meshRenderers)
                {
                    renderer.materials[1].SetFloat("_Alpha", 0f);
                }
                break;
            }
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
    
    public FlyModeState GetCurrentState() => state;

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
