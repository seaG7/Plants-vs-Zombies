using Data.Enums;

namespace Infrastructure.Services.Planting
{
    public interface IPlantingService
    {
        void SelectPlant(PlantType type);
        void TryPlantAtCursor();
    }
}