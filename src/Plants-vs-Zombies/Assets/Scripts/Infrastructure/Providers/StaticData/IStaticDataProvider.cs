using System.Collections.Generic;
using Data.Configs;
using Data.Enums;

namespace Infrastructure.Providers.StaticData
{
    public interface IStaticDataProvider
    {
        void Load();
        
        PlantData GetPlantData(PlantType type);
        EnemyData GetEnemyData(EnemyType type);
        LevelData GetLevelData(); 
        List<PlantData> GetAllPlants();
        List<EnemyData> GetAllEnemies();
        GameConfig GetGameConfig();
    }
}