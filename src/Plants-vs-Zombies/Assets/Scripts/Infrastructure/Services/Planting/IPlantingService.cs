using System;
using Data.Enums;
using UnityEngine;

namespace Infrastructure.Services.Planting
{
    public interface IPlantingService
    {
        event Action<PlantType> OnPlantSelected;
        event Action<Vector3> OnPlantingSuccess;
        void SelectPlant(PlantType type);
        void TryPlantAtCursor();
        void ClearSelection();
        void Initialize();
        void Dispose();
        void ShowTutorialHighlight(int lane, int row);
        void HideTutorialHighlight();
    }
}