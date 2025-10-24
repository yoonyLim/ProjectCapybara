using System;
using Moko;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public RCC_BasicMoveState basicMoveState { get; private set; }
    public RCC_SquashState squashState { get; private set; }
    public RCC_HeadbuttState headbuttState { get; private set; }
    public BaseState currentState;

    public PlayerAnimator playerAnimator { get; private set; }
    public PlayerAudioSourceHolder audioSourceHolder { get; private set; }
    public PlayerInput playerInput { get; private set; }
    public CharacterMotor motor { get; private set; }
    public Animator animator { get; private set; }
    public Rigidbody rb { get; private set; }
    public HeadbuttVFXPlayer headbuttVFXPlayer { get; private set; }
    
    public bool canMove;

    public GameObject rockBreaker;
    [HideInInspector] public float lastheadbuttUsagetime;
    public float headbuttCoolTime = 0.6f;

    public bool canHeadbutt = true;

    public Transform scaleTarget;

    private void Awake()
    {
        playerAnimator = GetComponent<PlayerAnimator>();
        animator = GetComponent<Animator>();
        motor = GetComponent<CharacterMotor>();
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        audioSourceHolder = GetComponentInChildren<PlayerAudioSourceHolder>();
        headbuttVFXPlayer = GetComponentInChildren<HeadbuttVFXPlayer>();
    }

    private void Start()
    {
        InitializeStateMachine();
        rockBreaker.SetActive(false);
    }

    private void Update()
    {
        currentState.OnUpdateState();

        if (Time.time - lastheadbuttUsagetime > headbuttCoolTime)
        {
            canHeadbutt = true;
        }
        else
        {
            canHeadbutt = false;
        }
    }

    private void FixedUpdate()
    {
        currentState.OnFixedUpdateState();
    }
    
    private void InitializeStateMachine()
    {
        basicMoveState = new RCC_BasicMoveState(this);
        squashState = new RCC_SquashState(this);
        headbuttState = new RCC_HeadbuttState(this);

        currentState = basicMoveState;
        currentState.OnEnterState();
    }
    
    public void ChangeState(BaseState newState)
    {
        currentState.OnExitState();
        currentState = newState;
        currentState.OnEnterState();
        Moko.DebugExtension.ColorLog($"currentState is {currentState}", "yellow");
    }

    public void Lunge(float force)
    {
        rb.AddForce(transform.forward * force, ForceMode.Impulse);
    }

    public void Squash()
    {
        Debug.Log("Squash");
        
        if (currentState != squashState)
            ChangeState(squashState);
        else
            squashState.AddSquashDuration();
    }

    public void PlayFootStepSound()
    {
            audioSourceHolder.footStepSound.PlayFootStepSound();
    }
}
