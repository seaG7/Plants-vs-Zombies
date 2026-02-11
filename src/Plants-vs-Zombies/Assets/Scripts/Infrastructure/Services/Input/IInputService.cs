using System;
using UI.Mobile;
using UnityEngine;

namespace Infrastructure.Services.Input
{
    public interface IInputService
    {
        Vector2 GetAimInput();
        Vector2 GetLookDelta();
        bool IsFirePressed();
        Vector2 GetPointerPosition();
        bool IsClickPressed();

        event Action OnFirePerformed;
        event Action OnClickPerformed;
        event Action OnCancelPerformed;
        event Action<int> OnHotbarHotkeyPressed; 

        void Enable();
        void Disable();
        
        void RegisterMobileControls(MobileControlsView controls);
        void TriggerHotbar(int index);
    }
}