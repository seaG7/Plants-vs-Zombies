using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Infrastructure.Services.Input
{
    public class InputService : IInputService, IDisposable
    {
        private readonly InputActions _inputActions;
        
        public event Action OnFirePerformed;
        public event Action OnClickPerformed;
        public event Action OnCancelPerformed;
        public event Action<int> OnHotbarHotkeyPressed;

        public InputService()
        {
            _inputActions = new InputActions();
            SubscribeEvents();
        }

        public void Enable() => _inputActions.Enable();
        public void Disable() => _inputActions.Disable();

        public Vector2 GetAimInput() => 
            _inputActions.Gameplay.Aim.ReadValue<Vector2>();

        public Vector2 GetPointerPosition() => 
            Mouse.current.position.ReadValue();

        public bool IsFirePressed() => 
            _inputActions.Gameplay.Fire.IsPressed();

        public bool IsClickPressed() => 
            _inputActions.Gameplay.Click.IsPressed();

        public void Dispose()
        {
            UnsubscribeEvents();
            _inputActions?.Dispose();
        }

        private void SubscribeEvents()
        {
            _inputActions.Gameplay.Fire.performed += FirePerformed;
            _inputActions.Gameplay.Click.performed += ClickPerformed;
            _inputActions.Gameplay.Cancel.performed += CancelPerformed;

            _inputActions.Gameplay.Hotbar.performed += HotbarPerformed;
        }

        private void UnsubscribeEvents()
        {
            _inputActions.Gameplay.Fire.performed -= FirePerformed;
            _inputActions.Gameplay.Click.performed -= ClickPerformed;
            _inputActions.Gameplay.Cancel.performed -= CancelPerformed;
            _inputActions.Gameplay.Hotbar.performed -= HotbarPerformed;
        }

        private void FirePerformed(InputAction.CallbackContext ctx) => OnFirePerformed?.Invoke();
        private void ClickPerformed(InputAction.CallbackContext ctx) => OnClickPerformed?.Invoke();
        private void CancelPerformed(InputAction.CallbackContext ctx) => OnCancelPerformed?.Invoke();

        private void HotbarPerformed(InputAction.CallbackContext ctx)
        {
            int keyIndex = (int)ctx.ReadValue<float>(); 
            if (keyIndex > 0) OnHotbarHotkeyPressed?.Invoke(keyIndex);
        }
    }
}