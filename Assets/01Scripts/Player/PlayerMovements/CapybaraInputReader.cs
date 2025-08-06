using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Capybara
{
    [CreateAssetMenu(fileName = "CapybaraInputReader", menuName = "CapybaraInputReader")]
    public class CapybaraInputReader : ScriptableObject, CapybaraInput.IGamePlayActions, CapybaraInput.IUIActions
    {
        private bool isInitialized;

        private CapybaraInput capybaraInput;

        public void Initialize()
        {
            capybaraInput = new CapybaraInput();
            
            capybaraInput.GamePlay.SetCallbacks(this);
            capybaraInput.UI.SetCallbacks(this);
            
            EnableGamePlayActionInputs();
        }

        /*public void Dispose()
        {
            if (capybaraInput == null)
                return;
                
            capybaraInput.GamePlay.Disable();
            capybaraInput.UI.Disable();
            capybaraInput = null;
        }*/

        public void EnableGamePlayActionInputs()
        {
            capybaraInput.UI.Disable();
            capybaraInput.GamePlay.Enable();
        }

        public void EnableUIActionInputs()
        {
            capybaraInput.GamePlay.Disable();
            capybaraInput.UI.Enable();
        }

        public event Action<Vector2> MoveEvent;
        public event Action<Vector2> MoveCanceledEvent;
        public event Action SprintEvent;
        public event Action SprintCanceledEvent;
        public event Action JumpEvent;
        public event Action JumpCanceledEvent;
        public event Action GlideEvent;
        public event Action GlideCanceledEvent;
        public event Action HeadbuttEvent;
        public event Action SoundwaveEvent;
        public event Action InteractEvent;
        public event Action PauseEvent;
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
            if (context.phase == InputActionPhase.Performed)
                SprintEvent?.Invoke();
            else if (context.phase == InputActionPhase.Canceled)
                SprintCanceledEvent?.Invoke();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            // Debug.Log($"Jump Phase: {context.phase}, Value: {context.ReadValueAsButton()}");
            if (context.phase == InputActionPhase.Performed)
                JumpEvent?.Invoke();
            else if  (context.phase == InputActionPhase.Canceled)
                JumpCanceledEvent?.Invoke();
        }

        public void OnGlide(InputAction.CallbackContext context)
        {
            // Debug.Log($"Glide Phase: {context.phase}, Value: {context.ReadValueAsButton()}");
            if (context.phase == InputActionPhase.Started)
                GlideEvent?.Invoke();
            else if (context.phase == InputActionPhase.Canceled)
                GlideCanceledEvent?.Invoke();
        }

        public void OnHeadbutt(InputAction.CallbackContext context)
        {
            // Debug.Log($"Headbutt Phase: {context.phase}, Value: {context.ReadValueAsButton()}");
            if (context.phase == InputActionPhase.Performed)
                HeadbuttEvent?.Invoke();
        }

        public void OnSoundwave(InputAction.CallbackContext context)
        {
            // Debug.Log($"Soundwave Phase: {context.phase}, Value: {context.ReadValueAsButton()}");
            if (context.phase == InputActionPhase.Performed)
                SoundwaveEvent?.Invoke();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            // Debug.Log($"Interact Phase: {context.phase}, Value: {context.ReadValueAsButton()}");
            if (context.phase == InputActionPhase.Performed)
                InteractEvent?.Invoke();
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            // Debug.Log($"Pause Phase: {context.phase}, Value: {context.ReadValueAsButton()}");
            if (context.phase == InputActionPhase.Performed)
            {
                PauseEvent?.Invoke();
                EnableUIActionInputs();
            }
        }

        public void OnResume(InputAction.CallbackContext context)
        {
            // Debug.Log($"Resume Phase: {context.phase}, Value: {context.ReadValueAsButton()}");
            if (context.phase == InputActionPhase.Performed)
            {
                ResumeEvent?.Invoke();
                EnableGamePlayActionInputs();
            }
        }
    }
}