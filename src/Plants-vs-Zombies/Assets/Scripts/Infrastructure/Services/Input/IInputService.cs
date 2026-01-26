using System;
using UnityEngine;

namespace Infrastructure.Services.Input
{
    public interface IInputService
    {
        Vector2 GetAimInput();
        bool IsFirePressed();

        Vector2 GetPointerPosition();
        bool IsClickPressed();

        event Action OnFirePerformed;
        event Action OnClickPerformed;
        event Action OnCancelPerformed;

        event Action<int> OnHotbarHotkeyPressed; 

        void Enable();
        void Disable();
    }
}