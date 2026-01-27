using System;
using Data.Configs;
using UnityEngine;

namespace Core.Interfaces
{
    public interface IPossessablePlant
    {
        Transform CameraMountPoint { get; }
        void Initialize(PlantData config);
        void SetPossessed(bool isPossessed);
        float GetReloadProgress();
        event Action OnFireFailedCooldown;
    }
}