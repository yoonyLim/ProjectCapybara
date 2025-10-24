using Capybara;
using UnityEngine;

namespace Moko
{
    public class PlayerInput : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public bool SprintInput { get; private set; }
        public bool JumpInput { get; private set; }
        public bool HeadbuttInput { get; private set; }

        [SerializeField] private CharacterMotor motor;
        [SerializeField] private CapybaraInputReader inputReader;
        private PlayerAnimator playerAnimator;
        private PlayerStateMachine stateMachine;
        
        #region Built-In Functions

        private void Awake()
        {
            motor = GetComponent<CharacterMotor>();
            playerAnimator = GetComponent<PlayerAnimator>();
            stateMachine = GetComponent<PlayerStateMachine>();
        }

        private void OnEnable()
        {
            if (inputReader != null)
            {
                inputReader.MoveEvent += OnMove;
                inputReader.MoveCanceledEvent += OnMoveCanceled;

                inputReader.SprintEvent += OnSprint;
                inputReader.SprintCanceledEvent += OnSprintCanceled;

                inputReader.JumpEvent += OnJump;
                
                inputReader.HeadbuttEvent += OnHeadbutt;

                inputReader.EnableGamePlayActionInputs();
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.MoveEvent -= OnMove;
                inputReader.MoveCanceledEvent -= OnMoveCanceled;

                inputReader.SprintEvent -= OnSprint;
                inputReader.SprintCanceledEvent -= OnSprintCanceled;

                inputReader.JumpEvent -= OnJump;
                
                inputReader.HeadbuttEvent -= OnHeadbutt;
            }
        }

        #endregion

        #region Call-Back Functions

        private void OnMove(Vector2 value)
        {
            MoveInput = value;
        }
        
        private void OnMoveCanceled(Vector2 value)
        {
            MoveInput = value;
        }

        private void OnSprint()
        {
            SprintInput = true;
            motor.MovementData.moveSpeed = motor.MovementData.sprintSpeed;
        }

        private void OnSprintCanceled()
        {
            SprintInput = false;
            motor.MovementData.moveSpeed = motor.MovementData.walkSpeed;
        }

        private void OnJump()
        {
            JumpInput = true;
        }

        private void OnHeadbutt()
        {
            HeadbuttInput = true;
        }
        #endregion

        #region ClearInput

        public void ClearJumpInput()
        {
            JumpInput = false;
        }
        
        public void ClearHeadbuttInput()
        {
            HeadbuttInput = false;
        }
        #endregion
    }
}
