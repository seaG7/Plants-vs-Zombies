using System;
using Data.Enums;

namespace Infrastructure.Services.Planting
{
    public interface IPlantingService
    {
        event Action<PlantType> OnPlantSelected;
        void SelectPlant(PlantType type);
        void TryPlantAtCursor();
        void ClearSelection();
        void Initialize();
    }
}