using System;
using UI.Mobile;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Infrastructure.Services.Input
{
    public class InputService : IInputService, IDisposable
    {
        private readonly InputActions _inputActions;
        private MobileControlsView _mobileControls;
        private bool _isMobileMode;
        private bool _wasFirePressedMobile;

        public event Action OnFirePerformed;
        public event Action OnClickPerformed;
        public event Action OnCancelPerformed;
        public event Action<int> OnHotbarHotkeyPressed;

        public InputService()
        {
            _inputActions = new InputActions();
            SubscribeEvents();
        }

        public void RegisterMobileControls(MobileControlsView controls)
        {
            _mobileControls = controls;
            _isMobileMode = _mobileControls != null && _mobileControls.gameObject.activeSelf;
        }

        public void TriggerHotbar(int index) => OnHotbarHotkeyPressed?.Invoke(index);

        public void Enable() => _inputActions.Enable();
        public void Disable() => _inputActions.Disable();

        public Vector2 GetAimInput()
        {
            if (_isMobileMode && _mobileControls != null)
                return _mobileControls.InputVector;
            return _inputActions.Gameplay.Aim.ReadValue<Vector2>();
        }
        
        public Vector2 GetLookDelta()
        {
            if (_isMobileMode) return Vector2.zero; 
            return Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
        }

        public Vector2 GetPointerPosition()
        {
            if (Mouse.current != null) return Mouse.current.position.ReadValue();
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
                return Touchscreen.current.primaryTouch.position.ReadValue();
            return Vector2.zero;
        }

        public bool IsFirePressed()
        {
            if (_isMobileMode && _mobileControls != null)
                return _mobileControls.IsFirePressed;
            return _inputActions.Gameplay.Fire.IsPressed();
        }

        public bool IsClickPressed() => _inputActions.Gameplay.Click.IsPressed();

        public void CheckMobileInput()
        {
             if (!_isMobileMode || _mobileControls == null) return;

             // 1. Logic for Exit Button (Consume logic to avoid flickering)
             if (_mobileControls.ConsumeExitPress())
             {
                 OnCancelPerformed?.Invoke();
             }

             // 2. Logic for Fire Button Event (Detect Edge)
             bool isFireDown = _mobileControls.IsFirePressed;
             if (isFireDown && !_wasFirePressedMobile)
             {
                 OnFirePerformed?.Invoke();
             }
             _wasFirePressedMobile = isFireDown;
        }

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
            if (int.TryParse(ctx.control.name, out int keyNumber)) OnHotbarHotkeyPressed?.Invoke(keyNumber);
            else { int val = (int)ctx.ReadValue<float>(); if (val > 0) OnHotbarHotkeyPressed?.Invoke(val); }
        }
    }
}