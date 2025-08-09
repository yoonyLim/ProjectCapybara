using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace Capybara
{
    [CreateAssetMenu(fileName = "CapybaraInputReader", menuName = "CapybaraInputReader")]
    public class CapybaraInputReader : ScriptableObject, CapybaraInput.IGamePlayActions, CapybaraInput.IUIActions
    {
        private bool isInitialized;

        private CapybaraInput capybaraInput;

        public void OnEnable()
        {
            if (capybaraInput == null)
            {
                capybaraInput = new CapybaraInput();
            
                capybaraInput.GamePlay.SetCallbacks(this);
                capybaraInput.UI.SetCallbacks(this);
                
                capybaraInput.GamePlay.Disable();
                capybaraInput.UI.Disable();
            }
        }

        /**
         * @Description: Enables in-game input actions, disabling other input actions
         */
        public void EnableGamePlayActionInputs()
        {
            capybaraInput.UI.Disable();
            capybaraInput.GamePlay.Enable();
        }
        
        /**
         * @Description: Enables UI input actions, disabling other input actions - to be used in Main|Settings|Pause Menus
         */
        public void EnableUIActionInputs()
        {
            capybaraInput.GamePlay.Disable();
            capybaraInput.UI.Enable();
        }

        /**
         * @Description: Called when the movement input is pressed down to change the player movement
         * @Params: Vector2 Input Direction
         */
        public event Action<Vector2> MoveEvent;
        
        /**
         * @Description: Called when the movement key is pressed up to end the player movement
         */
        public event Action<Vector2> MoveCanceledEvent;
        
        /**
         * @Description: Called when the sprint key is pressed down to start sprinting
         */
        public event Action SprintEvent;
        
        /**
         * @Description: Called when the sprint key is pressed up to end sprinting
         */
        public event Action SprintCanceledEvent;
        
        /**
         * @Description: Called when the jump key is pressed down to start jumping
         */
        public event Action JumpEvent;
        
        /**
         * @Description: Called when the jump key is pressed up to start falling
         */
        public event Action JumpCanceledEvent;
        
        /**
         * @Description: Called when the glide key is pressed down to start gliding while in the air
         */
        public event Action GlideEvent;
        
        /**
         * @Description: Called when the glide key is pressed up to end gliding
         */
        public event Action GlideCanceledEvent;
        
        /**
         * @Description: Called when the headbutt key is pressed down to start headbutting
         */
        public event Action HeadbuttEvent;
        
        /**
         * @Description: Called when the soundwave key is pressed down to start soundwave ability
         */
        public event Action SoundwaveEvent;
        
        /**
         * @Description: Called when the interact key is pressed down to start interacting with near interactables
         */
        public event Action InteractEvent;
        
        /**
         * @Description: Called when the pause key is pressed down to pause the game and show Pause UI
         */
        public event Action PauseEvent;
        
        /**
         * @Description: Called when the resume key is pressed down to exit Pause UI and resume the game
         */
        public event Action ResumeEvent;

        public void OnMove(InputAction.CallbackContext context)
        {
            // Debug.Log($"Move Phase: {context.phase}, Value: {context.ReadValue<Vector2>()}");
            if (context.phase == InputActionPhase.Performed)
                MoveEvent?.Invoke(context.ReadValue<Vector2>());
            else if (context.phase == InputActionPhase.Canceled)
                MoveCanceledEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            // Debug.Log($"Sprint Phase: {context.phase}, Value: {context.ReadValueAsButton()}");
            if (context.phase == InputActionPhase.Started)
                SprintEvent?.Invoke();
            else if (context.phase == InputActionPhase.Canceled)
                SprintCanceledEvent?.Invoke();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            // Debug.Log($"Jump Phase: {context.phase}, Value: {context.ReadValueAsButton()}");
            if (context.phase == InputActionPhase.Started)
                JumpEvent?.Invoke();
            else if (context.phase == InputActionPhase.Performed && context.interaction is HoldInteraction && context.ReadValueAsButton())
            {
                Debug.Log($"Glide Phase: {context.phase}, Value: {context.ReadValueAsButton()}");
                GlideEvent?.Invoke();
            }
            else if  (context.phase == InputActionPhase.Canceled)
                JumpCanceledEvent?.Invoke();
        }

        public void OnGlide(InputAction.CallbackContext context)
        {
            // Debug.Log($"Glide Phase: {context.phase}, Value: {context.ReadValueAsButton()}");
        }

        public void OnHeadbutt(InputAction.CallbackContext context)
        {
            // Debug.Log($"Headbutt Phase: {context.phase}, Value: {context.ReadValueAsButton()}");
            if (context.phase == InputActionPhase.Started)
                HeadbuttEvent?.Invoke();
        }

        public void OnSoundwave(InputAction.CallbackContext context)
        {
            // Debug.Log($"Soundwave Phase: {context.phase}, Value: {context.ReadValueAsButton()}");
            if (context.phase == InputActionPhase.Started)
                SoundwaveEvent?.Invoke();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            // Debug.Log($"Interact Phase: {context.phase}, Value: {context.ReadValueAsButton()}");
            if (context.phase == InputActionPhase.Started)
                InteractEvent?.Invoke();
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            // Debug.Log($"Pause Phase: {context.phase}, Value: {context.ReadValueAsButton()}");
            if (context.phase == InputActionPhase.Started)
            {
                PauseEvent?.Invoke();
                EnableUIActionInputs();
            }
        }

        public void OnResume(InputAction.CallbackContext context)
        {
            // Debug.Log($"Resume Phase: {context.phase}, Value: {context.ReadValueAsButton()}");
            if (context.phase == InputActionPhase.Started)
            {
                ResumeEvent?.Invoke();
                EnableGamePlayActionInputs();
            }
        }
    }
}